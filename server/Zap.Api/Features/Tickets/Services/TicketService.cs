
using Microsoft.EntityFrameworkCore;
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
}
