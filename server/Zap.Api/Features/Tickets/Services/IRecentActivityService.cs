using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public interface IRecentActivityService
{
    Task<List<RecentActivityDto>> GetRecentActivityAsync(CurrentUser currentUser, int limit = 5);
}

public static class RecentActivityTypes
{
    public const string TicketCreated = "ticketCreated";
    public const string StatusChanged = "statusChanged";
    public const string PriorityChanged = "priorityChanged";
    public const string AssigneeChanged = "assigneeChanged";
    public const string CommentAdded = "commentAdded";
}

public record RecentActivityDto(
    string Id,
    string TicketId,
    string TicketName,
    string ProjectId,
    string Type,
    MemberInfoDto Actor,
    DateTime OccurredAt,
    string? Message,
    string? OldValue,
    string? NewValue,
    string? RelatedEntityName
)
{
    public string DisplayId => BasicTicketDto.FormatDisplayId(TicketId);
}
