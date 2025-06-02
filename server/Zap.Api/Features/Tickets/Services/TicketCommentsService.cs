using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public class TicketCommentsService(AppDbContext db) : ITicketCommentsService
{
    public async Task CreateCommentAsync(string senderId, string ticketId, string message)
    {
        await db.TicketComments.AddAsync(new TicketComment
        {
            SenderId = senderId,
            TicketId = ticketId,
            Message = message,
        });
        await db.SaveChangesAsync();
    }

    public async Task<List<CommentDto>> GetCommentsAsync(string ticketId)
    {
        return await db.TicketComments
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(
                        c.Id,
                        c.TicketId,
                        c.Message,
                        new MemberInfoDto(
                            c.SenderId,
                            c.Sender.User.FullName,
                            c.Sender.User.AvatarUrl,
                            c.Sender.Role.Name
                            ),
                        c.CreatedAt,
                        c.UpdatedAt
            ))
            .ToListAsync();
    }
}

