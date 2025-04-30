using Microsoft.AspNetCore.Authorization;

namespace Zap.Api.Common.Authorization;

internal class CompanyRolesAuthorizationRequirement : IAuthorizationRequirement
{
    public IEnumerable<string> AllowedRoles { get; }
    public CompanyRolesAuthorizationRequirement(IEnumerable<string> allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

internal class CompanyRolesAuthorizationHandler : AuthorizationHandler<CompanyRolesAuthorizationRequirement>
{
    public readonly CurrentUser _currentUser;

    public CompanyRolesAuthorizationHandler(CurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CompanyRolesAuthorizationRequirement requirement)
    {
        if (_currentUser.Member != null)
        {
            var found = false;
            foreach (var role in requirement.AllowedRoles)
            {
                if (_currentUser.Member.Role?.Name == role)
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }

}
