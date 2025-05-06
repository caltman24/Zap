using Microsoft.AspNetCore.Authorization;

namespace Zap.Api.Common.Authorization;

internal static class AuthorizationHandlerExtensions
{
    internal static AuthorizationBuilder AddCurrentUserHandler(this AuthorizationBuilder builder)
    {
        builder.Services.AddScoped<IAuthorizationHandler, CheckCurrentUserAuthHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CheckCurrentUserAuthHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, CompanyRolesAuthorizationHandler>();

        return builder;
    }

    internal static AuthorizationPolicyBuilder RequireCurrentUser(this AuthorizationPolicyBuilder builder)
    {
        return builder.RequireAuthenticatedUser()
            .AddRequirements(new CheckCurrentUserRequirement());
    }

    internal static AuthorizationPolicyBuilder RequireCompanyMember(this AuthorizationPolicyBuilder builder, params string[] roles)
    {
        builder.RequireAuthenticatedUser()
            .AddRequirements(new CheckCurrentUserRequirement())
            .AddRequirements(new CheckCurrentMemberRequirement());

        if (roles.Any())
        {
            builder.AddRequirements(new CompanyRolesAuthorizationRequirement(roles));
        }

        return builder;
    }

    public static RouteHandlerBuilder WithCompanyMember(this RouteHandlerBuilder builder, params string[] roles)
    {
        return builder.RequireAuthorization(pb =>
        {
            pb.RequireAuthenticatedUser();
            pb.RequireCompanyMember(roles);
            pb.Build();
        });
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
            if (currentUser.User != null)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    private class CheckCurrentMemberRequirement : IAuthorizationRequirement
    {
    }
    private class CheckCurrentMemberAuthHandler(CurrentUser currentUser)
        : AuthorizationHandler<CheckCurrentMemberRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CheckCurrentMemberRequirement requirement)
        {
            if (currentUser.Member != null && currentUser.CompanyId != null)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    private class CompanyRolesAuthorizationRequirement : IAuthorizationRequirement
    {
        public IEnumerable<string> AllowedRoles { get; }
        public CompanyRolesAuthorizationRequirement(IEnumerable<string> allowedRoles)
        {
            AllowedRoles = allowedRoles;
        }
    }
    private class CompanyRolesAuthorizationHandler : AuthorizationHandler<CompanyRolesAuthorizationRequirement>
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
}
