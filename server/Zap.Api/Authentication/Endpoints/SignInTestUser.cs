using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Zap.Api.Common;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Extensions;
using Zap.Api.Data.Models;

namespace Zap.Api.Authentication.Endpoints;

public class SignInTestUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/signin-test", Handle);
    
    private static async Task<Results<BadRequest<IEnumerable<IdentityError>>, SignInHttpResult>> Handle(
        SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, ILogger<Program> logger)
    {
        var user = await userManager.FindByEmailAsync("test@test.com");
        if (user != null)
        {
            var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);

            return TypedResults.SignIn(claimsPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
        }

        user = new AppUser
        {
            Email = "test@test.com",
            UserName = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true
        };
        user.SetDefaultAvatar();

        var res = await userManager.CreateAsync(user, "Password1!");
        if (!res.Succeeded) return TypedResults.BadRequest(res.Errors);

        await userManager.AddCustomClaimsAsync(user);
        await userManager.AddToRoleAsync(user, RoleNames.Admin);

        logger.LogDebug("Created test user {Email} with password Password1! and Role {Role}",
            user.Email, RoleNames.Admin);

        var principal = await signInManager.CreateUserPrincipalAsync(user);

        return TypedResults.SignIn(principal, authenticationScheme: IdentityConstants.BearerScheme);
    }
}