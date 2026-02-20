using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketCommentsService
{
    Task CreateCommentAsync(
        string senderId,
        string ticketId,
        string message
    );

    Task<List<CommentDto>> GetCommentsAsync(string ticketId);
    Task<bool> DeleteCommentAsync(string ticketId, string commentId, string requestingUserId);
    Task<bool> UpdateCommentAsync(string ticketId, string commentId, string message, string requestingUserId);
}

public record CommentDto(
    string Id,
    string TicketId,
    string Message,
    MemberInfoDto Sender,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
