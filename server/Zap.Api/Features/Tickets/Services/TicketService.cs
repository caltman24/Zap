using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Enums;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public class TicketService : ITicketService
{
    private const float DisplayIdExactScore = 1000;
    private const float DisplayIdPrefixScore = 850;
    private const float DisplayIdPartialScore = 700;
    private const float NamePrefixScore = 500;
    private const float NameTokenPrefixBaseScore = 400;
    private const float NameTokenPrefixRankMultiplier = 100;

    private readonly AppDbContext _db;
    private readonly ITicketHistoryService _historyService;

    public TicketService(AppDbContext db, ITicketHistoryService historyService)
    {
        _db = db;
        _historyService = historyService;
    }

    public async Task<CreateTicketResult> CreateTicketAsync(CreateTicketDto ticket, string creatorId)
    {
        var result = await _db.Tickets.AddAsync(new Ticket
        {
            Name = ticket.Name,
            Description = ticket.Description,
            ProjectId = ticket.ProjectId,
            PriorityId = (await _db.TicketPriorities.FirstAsync(p => p.Name.ToLower() == ticket.Priority.ToLower())).Id,
            StatusId = (await _db.TicketStatuses.FirstAsync(p => p.Name.ToLower() == ticket.Status.ToLower())).Id,
            TypeId = (await _db.TicketTypes.FirstAsync(p => p.Name.ToLower() == ticket.Type.ToLower())).Id,
            SubmitterId = ticket.SubmitterId
        });
        await _db.SaveChangesAsync();

        // Create history entry for ticket creation
        await _historyService.CreateHistoryEntryAsync(
            result.Entity.Id,
            creatorId,
            TicketHistoryTypes.Created
        );

        return await _db.Tickets
            .Where(t => t.Id == result.Entity.Id)
            .Select(newTicket => new CreateTicketResult(newTicket.Id))
            .FirstAsync();
    }

    public async Task DeleteTicketAsync(string ticketId)
    {
        var ticket = await _db.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null) return;

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
    }

    public async Task<List<BasicTicketDto>> GetAssignedTicketsAsync(string memberId)
    {
        return await ProjectBasicTickets(_db.Tickets
                .Where(t => t.AssigneeId == memberId || t.SubmitterId == memberId)
            )
            .ToListAsync();
    }

    public async Task<List<BasicTicketDto>> GetOpenTicketsAsync(string memberId, string roleName, string companyId)
    {
        return await ProjectBasicTickets(GetVisibleTicketsQuery(memberId, roleName, companyId)
                .Where(t => !t.IsArchived && !t.Project.IsArchived && t.Status.Name != TicketStatuses.Resolved))
            .ToListAsync();
    }

    public async Task<List<BasicTicketDto>> GetArchivedTicketsAsync(string memberId, string roleName, string companyId)
    {
        return await ProjectBasicTickets(GetVisibleTicketsQuery(memberId, roleName, companyId)
                .Where(t => t.IsArchived))
            .ToListAsync();
    }

    public async Task<List<BasicTicketDto>> GetResolvedTicketsAsync(string memberId, string roleName, string companyId)
    {
        return await ProjectBasicTickets(GetVisibleTicketsQuery(memberId, roleName, companyId)
                .Where(t => !t.IsArchived && !t.Project.IsArchived && t.Status.Name == TicketStatuses.Resolved))
            .ToListAsync();
    }

    public async Task<List<TicketSearchDto>> SearchVisibleTicketsAsync(
        string memberId,
        string roleName,
        string companyId,
        string searchTerm,
        int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || limit <= 0) return [];

        var trimmedSearchTerm = searchTerm.Trim();
        var namePrefixPattern = $"{trimmedSearchTerm}%";
        var nameTokenPrefixQuery = BuildNameTokenPrefixQuery(trimmedSearchTerm);
        var normalizedDisplayIdSearch = NormalizeDisplayIdSearchTerm(trimmedSearchTerm);
        var canonicalDisplayId = GetCanonicalDisplayId(normalizedDisplayIdSearch);
        var displayIdPrefixPattern = normalizedDisplayIdSearch != null && StartsWithDisplayIdPrefix(trimmedSearchTerm)
            ? $"{GetCanonicalDisplayIdPrefix(normalizedDisplayIdSearch)}%"
            : null;

        if (nameTokenPrefixQuery == null && normalizedDisplayIdSearch == null) return [];

        return await GetVisibleTicketsQuery(memberId, roleName, companyId)
            .Where(ticket => !ticket.IsArchived && !ticket.Project.IsArchived)
            .Select(ticket => new
            {
                ticket.Id,
                ticket.ProjectId,
                ticket.Name,
                ticket.DisplayId,
                NormalizedDisplayId = ticket.DisplayId.Replace("#", string.Empty).Replace("-", string.Empty),
                NameVector = EF.Functions.ToTsVector("simple", ticket.Name)
            })
            .Select(ticket => new
            {
                ticket.Id,
                ticket.ProjectId,
                ticket.Name,
                ticket.DisplayId,
                DisplayIdExactMatch = canonicalDisplayId != null && ticket.DisplayId == canonicalDisplayId,
                DisplayIdPrefixMatch = displayIdPrefixPattern != null &&
                                       EF.Functions.ILike(ticket.DisplayId, displayIdPrefixPattern),
                DisplayIdPartialMatch = normalizedDisplayIdSearch != null &&
                                        (EF.Functions.ILike(ticket.DisplayId, $"%{trimmedSearchTerm}%") ||
                                         EF.Functions.ILike(ticket.NormalizedDisplayId,
                                             $"%{normalizedDisplayIdSearch}%")),
                NamePrefixMatch = EF.Functions.ILike(ticket.Name, namePrefixPattern),
                NameTokenPrefixMatch = nameTokenPrefixQuery != null &&
                                       ticket.NameVector.Matches(EF.Functions.ToTsQuery("simple",
                                           nameTokenPrefixQuery)),
                NameTokenPrefixRank = nameTokenPrefixQuery != null
                    ? ticket.NameVector.Rank(EF.Functions.ToTsQuery("simple", nameTokenPrefixQuery))
                    : 0
            })
            .Where(ticket =>
                ticket.DisplayIdExactMatch ||
                ticket.DisplayIdPrefixMatch ||
                ticket.DisplayIdPartialMatch ||
                ticket.NamePrefixMatch ||
                ticket.NameTokenPrefixMatch)
            .Select(ticket => new
            {
                Score =
                    (ticket.DisplayIdExactMatch ? DisplayIdExactScore : 0) +
                    (ticket.DisplayIdPrefixMatch ? DisplayIdPrefixScore : 0) +
                    (ticket.DisplayIdPartialMatch ? DisplayIdPartialScore : 0) +
                    (ticket.NamePrefixMatch ? NamePrefixScore : 0) +
                    (ticket.NameTokenPrefixMatch
                        ? NameTokenPrefixBaseScore + ticket.NameTokenPrefixRank * NameTokenPrefixRankMultiplier
                        : 0),
                ticket.Id,
                ticket.ProjectId,
                ticket.Name,
                StoredDisplayId = ticket.DisplayId
            })
            .OrderByDescending(ticket => ticket.Score)
            .ThenBy(ticket => ticket.Name)
            .Take(limit)
            .Select(ticket => new TicketSearchDto(ticket.Id, ticket.ProjectId, ticket.Name)
            {
                Score = ticket.Score,
                StoredDisplayId = ticket.StoredDisplayId
            })
            .ToListAsync();
    }

    public async Task<BasicTicketDto?> GetTicketByIdAsync(string ticketId)
    {
        return await ProjectBasicTickets(_db.Tickets
                .Where(t => t.Id == ticketId))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsigneeAsync(string ticketId, string? memberId, string updaterId)
    {
        // Get current assignee info for history
        var currentTicket = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Include(t => t.Assignee)
            .ThenInclude(a => a!.User)
            .FirstOrDefaultAsync();

        if (currentTicket == null) return false;

        var oldAssigneeName = currentTicket.Assignee?.User.FullName;

        currentTicket.AssigneeId = memberId;
        currentTicket.UpdatedAt = DateTime.UtcNow;

        var rowsChanged = await _db.SaveChangesAsync();

        if (rowsChanged > 0)
        {
            // Get new assignee name for history
            string? newAssigneeName = null;
            if (memberId != null)
                newAssigneeName = await _db.CompanyMembers
                    .Where(m => m.Id == memberId)
                    .Select(m => m.User.FullName)
                    .FirstOrDefaultAsync();

            // Create history entry
            if (memberId == null)
                // Developer removed
                await _historyService.CreateHistoryEntryAsync(
                    ticketId,
                    updaterId,
                    TicketHistoryTypes.DeveloperRemoved,
                    relatedEntityName: oldAssigneeName
                );
            else
                // Developer assigned
                await _historyService.CreateHistoryEntryAsync(
                    ticketId,
                    updaterId,
                    TicketHistoryTypes.DeveloperAssigned,
                    relatedEntityName: newAssigneeName,
                    relatedEntityId: memberId
                );
        }

        return rowsChanged > 0;
    }

    public async Task<bool> UpdatePriorityAsync(string ticketId, string priority, string updaterId)
    {
        // Get current priority for history
        var currentTicket = await _db.Tickets
            .Include(t => t.Priority)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (currentTicket == null) return false;

        var priorityEntity = await _db.TicketPriorities
            .FirstOrDefaultAsync(p => p.Name == priority);

        if (priorityEntity == null) return false;

        var oldPriority = currentTicket.Priority.Name;

        currentTicket.PriorityId = priorityEntity.Id;
        currentTicket.UpdatedAt = DateTime.UtcNow;

        var rowsChanged = await _db.SaveChangesAsync();

        if (rowsChanged > 0 && oldPriority != priority)
            await _historyService.CreateHistoryEntryAsync(
                ticketId,
                updaterId,
                TicketHistoryTypes.UpdatePriority,
                oldPriority,
                priority
            );

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateStatusAsync(string ticketId, string status, string updaterId)
    {
        // Get current status for history
        var currentTicket = await _db.Tickets
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (currentTicket == null) return false;

        var statusEntity = await _db.TicketStatuses
            .FirstOrDefaultAsync(s => s.Name == status);

        if (statusEntity == null) return false;

        var oldStatus = currentTicket.Status.Name;

        currentTicket.StatusId = statusEntity.Id;
        currentTicket.UpdatedAt = DateTime.UtcNow;

        var rowsChanged = await _db.SaveChangesAsync();

        if (rowsChanged > 0 && oldStatus != status)
        {
            // Check if ticket is being resolved
            if (status == TicketStatuses.Resolved)
                await _historyService.CreateHistoryEntryAsync(
                    ticketId,
                    updaterId,
                    TicketHistoryTypes.Resolved
                );
            else
                await _historyService.CreateHistoryEntryAsync(
                    ticketId,
                    updaterId,
                    TicketHistoryTypes.UpdateStatus,
                    oldStatus,
                    status
                );
        }

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateTicketAsync(string ticketId, UpdateTicketDto ticket, string updaterId)
    {
        // Get current values for history tracking
        var currentTicket = await _db.Tickets
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Type)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (currentTicket == null) return false;

        var statusEntity = await _db.TicketStatuses.FirstOrDefaultAsync(s => s.Name == ticket.Status);
        var typeEntity = await _db.TicketTypes.FirstOrDefaultAsync(t => t.Name == ticket.Type);
        var priorityEntity = await _db.TicketPriorities.FirstOrDefaultAsync(p => p.Name == ticket.Priority);

        if (statusEntity == null || typeEntity == null || priorityEntity == null) return false;

        var oldName = currentTicket.Name;
        var oldDescription = currentTicket.Description;
        var oldPriority = currentTicket.Priority.Name;
        var oldStatus = currentTicket.Status.Name;
        var oldType = currentTicket.Type.Name;

        currentTicket.StatusId = statusEntity.Id;
        currentTicket.TypeId = typeEntity.Id;
        currentTicket.PriorityId = priorityEntity.Id;
        currentTicket.Name = ticket.Name;
        currentTicket.Description = ticket.Description;
        currentTicket.UpdatedAt = DateTime.UtcNow;

        var rowsChanged = await _db.SaveChangesAsync();

        if (rowsChanged > 0)
        {
            // Track individual field changes
            if (oldName != ticket.Name)
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateName,
                    oldName, ticket.Name);

            if (oldDescription != ticket.Description)
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateDescription);

            if (oldPriority != ticket.Priority)
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdatePriority,
                    oldPriority, ticket.Priority);

            if (oldStatus != ticket.Status)
            {
                if (ticket.Status == TicketStatuses.Resolved)
                    await _historyService.CreateHistoryEntryAsync(
                        ticketId, updaterId, TicketHistoryTypes.Resolved);
                else
                    await _historyService.CreateHistoryEntryAsync(
                        ticketId, updaterId, TicketHistoryTypes.UpdateStatus,
                        oldStatus, ticket.Status);
            }

            if (oldType != ticket.Type)
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateType,
                    oldType, ticket.Type);
        }

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateTypeAsync(string ticketId, string type, string updaterId)
    {
        // Get current type for history
        var currentTicket = await _db.Tickets
            .Include(t => t.Type)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (currentTicket == null) return false;

        var typeEntity = await _db.TicketTypes
            .FirstOrDefaultAsync(t => t.Name == type);

        if (typeEntity == null) return false;

        var oldType = currentTicket.Type.Name;

        currentTicket.TypeId = typeEntity.Id;
        currentTicket.UpdatedAt = DateTime.UtcNow;

        var rowsChanged = await _db.SaveChangesAsync();

        if (rowsChanged > 0 && oldType != type)
            await _historyService.CreateHistoryEntryAsync(
                ticketId,
                updaterId,
                TicketHistoryTypes.UpdateType,
                oldType,
                type
            );

        return rowsChanged > 0;
    }

    public async Task<bool> ValidateProjectManagerAsync(string ticketId, string memberId)
    {
        var pmId = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => t.Project.ProjectManagerId)
            .FirstOrDefaultAsync();

        return pmId == memberId;
    }

    public async Task<bool> ValidateAssignedMemberAsync(string ticketId, string memberId)
    {
        var ids = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t =>
                new
                {
                    t.AssigneeId,
                    t.SubmitterId
                })
            .FirstOrDefaultAsync();

        return memberId == ids?.AssigneeId || memberId == ids?.SubmitterId;
    }

    public async Task<bool> ValidateAssigneeAsync(string ticketId, string memberId)
    {
        var memberRole = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .SelectMany(t => t.Project.AssignedMembers)
            .Where(am => am.Id == memberId)
            .Select(am => am.Role.Name)
            .FirstOrDefaultAsync();

        return memberRole == RoleNames.Developer;
    }

    public async Task<bool> ValidateCompanyAsync(string ticketId, string? companyId)
    {
        var ticketCompanyId = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => t.Project.CompanyId)
            .FirstOrDefaultAsync();

        return ticketCompanyId != null && ticketCompanyId == companyId;
    }

    public async Task<List<MemberInfoDto>> GetProjectDevelopersAsync(string ticketId)
    {
        return await _db.Tickets
            .Where(t => t.Id == ticketId)
            .SelectMany(t => t.Project.AssignedMembers)
            .Where(am => am.Role.Name == RoleNames.Developer)
            .Select(am => new MemberInfoDto(
                am.Id,
                $"{am.User.FirstName} {am.User.LastName}",
                am.User.AvatarUrl,
                am.Role.Name
            ))
            .ToListAsync();
    }

    public async Task<bool> ToggleArchiveTicket(string ticketId, string updaterId)
    {
        var ticket = await _db.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null) return false;

        var isCurrentlyArchived = ticket.IsArchived;

        ticket.IsArchived = !ticket.IsArchived;
        ticket.UpdatedAt = DateTime.UtcNow;

        var rowsChanged = await _db.SaveChangesAsync();

        if (rowsChanged > 0)
        {
            var historyType = isCurrentlyArchived
                ? TicketHistoryTypes.Unarchived
                : TicketHistoryTypes.Archived;

            await _historyService.CreateHistoryEntryAsync(
                ticketId,
                updaterId,
                historyType
            );
        }

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateArchivedTicketAsync(string ticketId, string name, string description,
        string updaterId)
    {
        // Get current values for history tracking
        var currentTicket = await _db.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.IsArchived);

        if (currentTicket == null) return false;

        var oldName = currentTicket.Name;
        var oldDescription = currentTicket.Description;

        currentTicket.Name = name;
        currentTicket.Description = description;
        currentTicket.UpdatedAt = DateTime.UtcNow;

        var rowsChanged = await _db.SaveChangesAsync();

        if (rowsChanged > 0)
        {
            // Track individual field changes
            if (oldName != name)
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateName,
                    oldName, name);

            if (oldDescription != description)
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateDescription);
        }

        return rowsChanged > 0;
    }

    private IQueryable<Ticket> GetVisibleTicketsQuery(string memberId, string roleName, string companyId)
    {
        var query = _db.Tickets
            .AsNoTracking()
            .Where(t => t.Project.CompanyId == companyId);

        return roleName switch
        {
            RoleNames.Admin => query,
            RoleNames.ProjectManager => query.Where(t => t.Project.ProjectManagerId == memberId),
            RoleNames.Developer => query.Where(t => t.Project.AssignedMembers.Any(m => m.Id == memberId)),
            RoleNames.Submitter => query.Where(t =>
                t.Project.AssignedMembers.Any(m => m.Id == memberId) || t.SubmitterId == memberId),
            _ => query.Where(_ => false)
        };
    }

    private static string NormalizeSearchTerm(string searchTerm)
    {
        return new string(searchTerm
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());
    }

    private static string? NormalizeDisplayIdSearchTerm(string searchTerm)
    {
        var normalizedSearchTerm = NormalizeSearchTerm(searchTerm);

        if (normalizedSearchTerm.StartsWith("ZAP", StringComparison.Ordinal))
            normalizedSearchTerm = normalizedSearchTerm[3..];

        return string.IsNullOrWhiteSpace(normalizedSearchTerm)
            ? null
            : normalizedSearchTerm;
    }

    private static string? BuildNameTokenPrefixQuery(string searchTerm)
    {
        var tokens = searchTerm
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeSearchTerm)
            .Where(token => token.Length > 0)
            .Select(token => $"{token}:*")
            .ToList();

        return tokens.Count == 0
            ? null
            : string.Join(" & ", tokens);
    }

    private static string? GetCanonicalDisplayId(string? normalizedDisplayIdSearch)
    {
        return normalizedDisplayIdSearch is { Length: 4 }
            ? $"#ZAP-{normalizedDisplayIdSearch}"
            : null;
    }

    private static string GetCanonicalDisplayIdPrefix(string normalizedDisplayIdSearch)
    {
        return $"#ZAP-{normalizedDisplayIdSearch}";
    }

    private static bool StartsWithDisplayIdPrefix(string searchTerm)
    {
        return NormalizeSearchTerm(searchTerm).StartsWith("ZAP", StringComparison.Ordinal) ||
               searchTerm.Contains('#') ||
               searchTerm.Contains('-');
    }

    private static IQueryable<BasicTicketDto> ProjectBasicTickets(IQueryable<Ticket> query)
    {
        return query.Select(t => new BasicTicketDto(
            t.Id,
            t.Name,
            t.Description,
            t.Priority.Name,
            t.Status.Name,
            t.Type.Name,
            t.ProjectId,
            t.Project.ProjectManagerId,
            t.IsArchived,
            t.Project.IsArchived,
            t.CreatedAt,
            t.UpdatedAt,
            new MemberInfoDto(
                t.Submitter.Id,
                $"{t.Submitter.User.FirstName} {t.Submitter.User.LastName}",
                t.Submitter.User.AvatarUrl,
                t.Submitter.Role.Name),
            t.Assignee == null
                ? null
                : new MemberInfoDto(
                    t.Assignee.Id,
                    $"{t.Assignee.User.FirstName} {t.Assignee.User.LastName}",
                    t.Assignee.User.AvatarUrl,
                    t.Assignee.Role.Name)
        )
        {
            StoredDisplayId = t.DisplayId
        });
    }
}