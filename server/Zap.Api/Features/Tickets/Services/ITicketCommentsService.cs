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
    Task<bool> DeleteCommentAsync(string commentId);
}

public record CommentDto(
        string Id,
        string TicketId,
        string Message,
        MemberInfoDto Sender,
        DateTime CreatedAt,
        DateTime? UpdatedAt
);
