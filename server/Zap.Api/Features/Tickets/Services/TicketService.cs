
using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;
using Zap.Api.Data.Models;

namespace Zap.Api.Features.Tickets.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;

    public TicketService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketDto ticket)
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

        var newTicket = result.Entity;

        return new TicketDto(
            newTicket.Id,
            newTicket.Name,
            newTicket.Description,
            newTicket.Priority.Name,
            newTicket.Status.Name,
            newTicket.Type.Name,
            newTicket.ProjectId,
            newTicket.SubmitterId,
            newTicket.AssigneeId
        );
    }

    public async Task<TicketDto?> GetTicketByIdAsync(string ticketId)
    {
        return await _db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => new TicketDto(
                t.Id,
                t.Name,
                t.Description,
                t.Priority.Name,
                t.Status.Name,
                t.Type.Name,
                t.ProjectId,
                t.SubmitterId,
                t.AssigneeId
            ))
            .FirstOrDefaultAsync();
    }
}
