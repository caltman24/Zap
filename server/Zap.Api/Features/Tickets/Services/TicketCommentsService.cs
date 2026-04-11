using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
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
            Message = message
        });
        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteCommentAsync(string ticketId, string commentId, CurrentUser currentUser)
    {
        if (currentUser.Member == null) return false;

        var comment = await db.TicketComments
            .Where(c => c.TicketId == ticketId && c.Id == commentId)
            .Select(c => new
            {
                Comment = c,
                c.SenderId,
                SenderRoleName = c.Sender.Role.Name,
                c.Ticket.Project.CompanyId,
                c.Ticket.Project.ProjectManagerId,
                c.Ticket.SubmitterId,
                c.Ticket.AssigneeId,
                c.Ticket.IsArchived
            })
            .FirstOrDefaultAsync();

        if (comment == null) return false;

        if (comment.CompanyId != currentUser.Member.CompanyId) return false;

        if (!CanDeleteComment(
                comment.SenderId,
                comment.SenderRoleName,
                comment.ProjectManagerId,
                comment.SubmitterId,
                comment.AssigneeId,
                currentUser)) return false;

        db.TicketComments.Remove(comment.Comment);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCommentAsync(string ticketId, string commentId, string message,
        CurrentUser currentUser)
    {
        if (currentUser.Member == null) return false;

        var comment = await db.TicketComments
            .Where(c => c.TicketId == ticketId && c.Id == commentId)
            .Select(c => new
            {
                Comment = c,
                c.SenderId,
                SenderRoleName = c.Sender.Role.Name,
                c.Ticket.Project.CompanyId,
                c.Ticket.Project.ProjectManagerId,
                c.Ticket.SubmitterId,
                c.Ticket.AssigneeId,
                c.Ticket.IsArchived
            })
            .FirstOrDefaultAsync();

        if (comment == null) return false;

        if (comment.CompanyId != currentUser.Member.CompanyId) return false;

        if (!CanEditComment(
                comment.SenderId,
                comment.ProjectManagerId,
                comment.SubmitterId,
                comment.AssigneeId,
                currentUser)) return false;

        comment.Comment.Message = message;
        comment.Comment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<CommentDto>> GetCommentsAsync(string ticketId, CurrentUser currentUser)
    {
        var currentMemberId = currentUser.Member?.Id;

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
                c.UpdatedAt,
                new CommentCapabilitiesDto(
                    CanEditComment(
                        c.SenderId,
                        c.Ticket.Project.ProjectManagerId,
                        c.Ticket.SubmitterId,
                        c.Ticket.AssigneeId,
                        currentUser),
                    CanDeleteComment(
                        c.SenderId,
                        c.Sender.Role.Name,
                        c.Ticket.Project.ProjectManagerId,
                        c.Ticket.SubmitterId,
                        c.Ticket.AssigneeId,
                        currentUser)
                )
            ))
            .ToListAsync();
    }

    private static bool CanEditComment(
        string senderId,
        string? projectManagerId,
        string submitterId,
        string? assigneeId,
        CurrentUser currentUser)
    {
        if (currentUser.Member == null) return false;

        return senderId == currentUser.Member.Id &&
               TicketAuthorizationRules.CanCommentOnTicket(projectManagerId, submitterId, assigneeId, currentUser);
    }

    private static bool CanDeleteComment(
        string senderId,
        string senderRoleName,
        string? projectManagerId,
        string submitterId,
        string? assigneeId,
        CurrentUser currentUser)
    {
        if (currentUser.Member == null) return false;

        if (currentUser.Member.Role.Name == RoleNames.Admin) return true;

        if (projectManagerId == currentUser.Member.Id)
            return senderRoleName != RoleNames.Admin;

        if (currentUser.Member.Role.Name is RoleNames.Developer or RoleNames.Submitter)
            return senderId == currentUser.Member.Id &&
                   TicketAuthorizationRules.CanCommentOnTicket(projectManagerId, submitterId, assigneeId, currentUser);

        if (senderId == currentUser.Member.Id)
            return TicketAuthorizationRules.CanCommentOnTicket(projectManagerId, submitterId, assigneeId, currentUser);

        return false;
    }
}