using Microsoft.AspNetCore.Authorization;

namespace Zap.Api.Common.Authorization;

internal static class AuthorizationHandlerExtensions
{
    internal static AuthorizationBuilder AddCurrentUserHandler(this AuthorizationBuilder builder)
    {
        builder.Services.AddScoped<IAuthorizationHandler, CheckCurrentUserAuthHandler>();

        // HACK: Just for now add this when registering the currentUser handler. Break out into seperate static class
        builder.Services.AddScoped<IAuthorizationHandler, CompanyRolesAuthorizationHandler>();
        return builder;
    }

    internal static AuthorizationPolicyBuilder RequireCurrentUser(this AuthorizationPolicyBuilder builder)
    {
        return builder.RequireAuthenticatedUser()
            .AddRequirements(new CheckCurrentUserRequirement());
    }

    internal static AuthorizationPolicyBuilder RequireCompanyRole(this AuthorizationPolicyBuilder builder, params string[] roles)
    {
        return builder.RequireAuthenticatedUser()
            .AddRequirements(new CompanyRolesAuthorizationRequirement(roles));
    }

    private class CheckCurrentUserRequirement : IAuthorizationRequirement
    {
    }

    private class CheckCurrentUserAuthHandler(CurrentUser currentUser)
        : AuthorizationHandler<CheckCurrentUserRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CheckCurrentUserRequirement requirement)
        {
            if (currentUser.User is not null)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
