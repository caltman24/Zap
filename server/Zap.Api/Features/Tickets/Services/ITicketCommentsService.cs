using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketCommentsService
{
    Task CreateCommentAsync(
        string senderId,
        string ticketId,
        string message
    );

    Task<List<CommentDto>> GetCommentsAsync(string ticketId, CurrentUser currentUser);
    Task<bool> DeleteCommentAsync(string ticketId, string commentId, CurrentUser currentUser);
    Task<bool> UpdateCommentAsync(string ticketId, string commentId, string message, CurrentUser currentUser);
}

public record CommentDto(
    string Id,
    string TicketId,
    string Message,
    MemberInfoDto Sender,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    CommentCapabilitiesDto Capabilities
);

public record CommentCapabilitiesDto(
    bool CanEdit,
    bool CanDelete
);