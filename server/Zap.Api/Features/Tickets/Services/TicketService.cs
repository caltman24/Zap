
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;

    public TicketService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CreateTicketResult> CreateTicketAsync(CreateTicketDto ticket)
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
        // FIXME: This should filter out resolved & archived tickets
        return await _db.Tickets
            .Where(t => t.Project.CompanyId == companyId)
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

    public async Task<bool> UpdateAsigneeAsync(string ticketId, string? memberId)
    {
        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(t => t.AssigneeId, memberId));

        return rowsChanged > 0;
    }

    public async Task<bool> UpdatePriorityAsync(string ticketId, string priority)
    {
        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter =>
                    setter.SetProperty(
                        t => t.PriorityId,
                        _db.TicketPriorities.First(s => s.Name == priority).Id));

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateStatusAsync(string ticketId, string status)
    {
        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter =>
                    setter.SetProperty(
                        t => t.StatusId,
                        _db.TicketStatuses.First(s => s.Name == status).Id));

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateTicketAsync(string ticketId, UpdateTicketDto ticket)
    {
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

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateTypeAsync(string ticketId, string type)
    {
        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter =>
                    setter.SetProperty(
                        t => t.TypeId,
                        _db.TicketTypes.First(t => t.Name == type).Id));

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

    public async Task<bool> ToggleArchiveTicket(string ticketId)
    {
        int rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(t => t.IsArchived, t => !t.IsArchived));

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateArchivedTicketAsync(string ticketId, string name, string description)
    {
        var rowsChanged = await _db.Tickets
            .Where(t => t.Id == ticketId && t.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Name, name)
                    .SetProperty(t => t.Description, description));

        return rowsChanged > 0;
    }
}
