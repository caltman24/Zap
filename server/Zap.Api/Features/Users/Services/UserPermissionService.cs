using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;

namespace Zap.Api.Features.Users.Services;

public sealed class UserPermissionService : IUserPermissionService
{
    private static readonly string[] AdminPermissions =
    [
        "company.edit",
        "project.create",
        "project.viewAll",
        "project.viewArchived",
        "project.assignPm",
        "ticket.create"
    ];

    private static readonly string[] ProjectManagerPermissions =
    [
        "project.create",
        "project.viewAll",
        "project.viewAssigned",
        "project.viewArchived",
        "ticket.create"
    ];

    private static readonly string[] DeveloperPermissions =
    [
        "project.viewAssigned"
    ];

    private static readonly string[] SubmitterPermissions =
    [
        "project.viewAssigned",
        "ticket.create"
    ];

    public IReadOnlyList<string> GetPermissions(CurrentUser currentUser)
    {
        var roleName = currentUser.Member?.Role?.Name;

        return roleName switch
        {
            RoleNames.Admin => AdminPermissions,
            RoleNames.ProjectManager => ProjectManagerPermissions,
            RoleNames.Developer => DeveloperPermissions,
            RoleNames.Submitter => SubmitterPermissions,
            _ => []
        };
    }
}
