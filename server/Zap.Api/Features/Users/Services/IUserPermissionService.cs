using Zap.Api.Common.Authorization;

namespace Zap.Api.Features.Users.Services;

public interface IUserPermissionService
{
    IReadOnlyList<string> GetPermissions(CurrentUser currentUser);
}
