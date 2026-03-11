using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;

namespace Zap.Api.Features.Tickets.Services;

internal static class TicketAuthorizationRules
{
    public static bool CanCommentOnTicket(
        string? projectManagerId,
        string submitterId,
        string? assigneeId,
        CurrentUser currentUser)
    {
        if (currentUser.Member == null) return false;

        return currentUser.Member.Role.Name switch
        {
            RoleNames.Admin => true,
            RoleNames.ProjectManager => projectManagerId == currentUser.Member.Id,
            RoleNames.Developer => assigneeId == currentUser.Member.Id,
            RoleNames.Submitter => submitterId == currentUser.Member.Id,
            _ => false
        };
    }
}
