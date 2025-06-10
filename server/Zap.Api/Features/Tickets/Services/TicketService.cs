
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Enums;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public class TicketService : ITicketService
{
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
        await _db.Tickets.Where(t => t.Id == ticketId).ExecuteDeleteAsync();
    }

    public async Task<List<BasicTicketDto>> GetAssignedTicketsAsync(string memberId)
    {
        return await _db.Tickets
            .Where(t => t.AssigneeId == memberId || t.SubmitterId == memberId)
            .Select(t => new BasicTicketDto(
                t.Id,
                t.Name,
                t.Description,
                t.Priority.Name,
                t.Status.Name,
                t.Type.Name,
                t.ProjectId,
                t.IsArchived,
                t.Project.IsArchived,
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
            ))
            .ToListAsync();
    }

    public async Task<List<BasicTicketDto>> GetOpenTicketsAsync(string companyId)
    {
        return await _db.Tickets
            .Where(t => t.Project.CompanyId == companyId &&
                       !t.IsArchived &&
                       !t.Project.IsArchived &&
                       t.Status.Name != TicketStatuses.Resolved)
            .Select(t => new BasicTicketDto(
                t.Id,
                t.Name,
                t.Description,
                t.Priority.Name,
                t.Status.Name,
                t.Type.Name,
                t.ProjectId,
                t.IsArchived,
                t.Project.IsArchived,
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
            ))
            .ToListAsync();
    }

    public async Task<List<BasicTicketDto>> GetArchivedTicketsAsync(string companyId)
    {
        return await _db.Tickets
            .Where(t => t.Project.CompanyId == companyId && t.IsArchived)
            .Select(t => new BasicTicketDto(
                t.Id,
                t.Name,
                t.Description,
                t.Priority.Name,
                t.Status.Name,
                t.Type.Name,
                t.ProjectId,
                t.IsArchived,
                t.Project.IsArchived,
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
            ))
            .ToListAsync();
    }

    public async Task<List<BasicTicketDto>> GetResolvedTicketsAsync(string companyId)
    {
        return await _db.Tickets
            .Where(t => t.Project.CompanyId == companyId &&
                       !t.IsArchived &&
                       !t.Project.IsArchived &&
                       t.Status.Name == TicketStatuses.Resolved)
            .Select(t => new BasicTicketDto(
                t.Id,
                t.Name,
                t.Description,
                t.Priority.Name,
                t.Status.Name,
                t.Type.Name,
                t.ProjectId,
                t.IsArchived,
                t.Project.IsArchived,
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
            ))
            .ToListAsync();
    }

    public async Task<BasicTicketDto?> GetTicketByIdAsync(string ticketId)
    {
        return await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => new BasicTicketDto(
                t.Id,
                t.Name,
                t.Description,
                t.Priority.Name,
                t.Status.Name,
                t.Type.Name,
                t.ProjectId,
                t.IsArchived,
                t.Project.IsArchived,
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
            ))
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

        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(t => t.AssigneeId, memberId));

        if (rowsChanged > 0)
        {
            // Get new assignee name for history
            string? newAssigneeName = null;
            if (memberId != null)
            {
                newAssigneeName = await _db.CompanyMembers
                    .Where(m => m.Id == memberId)
                    .Select(m => m.User.FullName)
                    .FirstOrDefaultAsync();
            }

            // Create history entry
            if (memberId == null)
            {
                // Developer removed
                await _historyService.CreateHistoryEntryAsync(
                    ticketId,
                    updaterId,
                    TicketHistoryTypes.DeveloperRemoved,
                    relatedEntityName: oldAssigneeName
                );
            }
            else
            {
                // Developer assigned
                await _historyService.CreateHistoryEntryAsync(
                    ticketId,
                    updaterId,
                    TicketHistoryTypes.DeveloperAssigned,
                    relatedEntityName: newAssigneeName,
                    relatedEntityId: memberId
                );
            }
        }

        return rowsChanged > 0;
    }

    public async Task<bool> UpdatePriorityAsync(string ticketId, string priority, string updaterId)
    {
        // Get current priority for history
        var oldPriority = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => t.Priority.Name)
            .FirstOrDefaultAsync();

        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter =>
                    setter.SetProperty(
                        t => t.PriorityId,
                        _db.TicketPriorities.First(s => s.Name == priority).Id));

        if (rowsChanged > 0 && oldPriority != priority)
        {
            await _historyService.CreateHistoryEntryAsync(
                ticketId,
                updaterId,
                TicketHistoryTypes.UpdatePriority,
                oldPriority,
                priority
            );
        }

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateStatusAsync(string ticketId, string status, string updaterId)
    {
        // Get current status for history
        var oldStatus = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => t.Status.Name)
            .FirstOrDefaultAsync();

        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter =>
                    setter.SetProperty(
                        t => t.StatusId,
                        _db.TicketStatuses.First(s => s.Name == status).Id));

        if (rowsChanged > 0 && oldStatus != status)
        {
            // Check if ticket is being resolved
            if (status == TicketStatuses.Resolved)
            {
                await _historyService.CreateHistoryEntryAsync(
                    ticketId,
                    updaterId,
                    TicketHistoryTypes.Resolved
                );
            }
            else
            {
                await _historyService.CreateHistoryEntryAsync(
                    ticketId,
                    updaterId,
                    TicketHistoryTypes.UpdateStatus,
                    oldStatus,
                    status
                );
            }
        }

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateTicketAsync(string ticketId, UpdateTicketDto ticket, string updaterId)
    {
        // Get current values for history tracking
        var currentTicket = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => new
            {
                t.Name,
                t.Description,
                Priority = t.Priority.Name,
                Status = t.Status.Name,
                Type = t.Type.Name
            })
            .FirstOrDefaultAsync();

        if (currentTicket == null) return false;

        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter => setter
                    // INFO: We Should always have the correct tickect types names. Should validate before update. maybe
                    // pass the ids from validation to prevent multiple db calls.
                    // Calling First() should never fail
                    .SetProperty(t => t.StatusId, _db.TicketStatuses.First(s => s.Name == ticket.Status).Id)
                    .SetProperty(t => t.TypeId, _db.TicketTypes.First(s => s.Name == ticket.Type).Id)
                    .SetProperty(t => t.PriorityId, _db.TicketPriorities.First(s => s.Name == ticket.Priority).Id)
                    .SetProperty(t => t.Name, ticket.Name)
                    .SetProperty(t => t.Description, ticket.Description));

        if (rowsChanged > 0)
        {
            // Track individual field changes
            if (currentTicket.Name != ticket.Name)
            {
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateName,
                    currentTicket.Name, ticket.Name);
            }

            if (currentTicket.Description != ticket.Description)
            {
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateDescription);
            }

            if (currentTicket.Priority != ticket.Priority)
            {
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdatePriority,
                    currentTicket.Priority, ticket.Priority);
            }

            if (currentTicket.Status != ticket.Status)
            {
                if (ticket.Status == TicketStatuses.Resolved)
                {
                    await _historyService.CreateHistoryEntryAsync(
                        ticketId, updaterId, TicketHistoryTypes.Resolved);
                }
                else
                {
                    await _historyService.CreateHistoryEntryAsync(
                        ticketId, updaterId, TicketHistoryTypes.UpdateStatus,
                        currentTicket.Status, ticket.Status);
                }
            }

            if (currentTicket.Type != ticket.Type)
            {
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateType,
                    currentTicket.Type, ticket.Type);
            }
        }

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateTypeAsync(string ticketId, string type, string updaterId)
    {
        // Get current type for history
        var oldType = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => t.Type.Name)
            .FirstOrDefaultAsync();

        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter =>
                    setter.SetProperty(
                        t => t.TypeId,
                        _db.TicketTypes.First(t => t.Name == type).Id));

        if (rowsChanged > 0 && oldType != type)
        {
            await _historyService.CreateHistoryEntryAsync(
                ticketId,
                updaterId,
                TicketHistoryTypes.UpdateType,
                oldType,
                type
            );
        }

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

        return (ticketCompanyId != null && ticketCompanyId == companyId);
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
        // Get current archive status for history
        var isCurrentlyArchived = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => t.IsArchived)
            .FirstOrDefaultAsync();

        int rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(t => t.IsArchived, t => !t.IsArchived));

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

    public async Task<bool> UpdateArchivedTicketAsync(string ticketId, string name, string description, string updaterId)
    {
        // Get current values for history tracking
        var currentTicket = await _db.Tickets
            .Where(t => t.Id == ticketId && t.IsArchived)
            .Select(t => new { t.Name, t.Description })
            .FirstOrDefaultAsync();

        if (currentTicket == null) return false;

        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId && t.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Name, name)
                    .SetProperty(t => t.Description, description));

        if (rowsChanged > 0)
        {
            // Track individual field changes
            if (currentTicket.Name != name)
            {
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateName,
                    currentTicket.Name, name);
            }

            if (currentTicket.Description != description)
            {
                await _historyService.CreateHistoryEntryAsync(
                    ticketId, updaterId, TicketHistoryTypes.UpdateDescription);
            }
        }

        return rowsChanged > 0;
    }
}
