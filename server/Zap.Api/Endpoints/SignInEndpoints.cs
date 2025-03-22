using Microsoft.AspNetCore.Identity;
using Zap.Api.Extensions;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

public static class SignInEndpoints
{
    public static IEndpointRouteBuilder MapSignInEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/signin");

        group.MapPost("/", async (SignInUserRequest request, SignInManager<AppUser> signInManager) =>
        {
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
            var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
            if (!result.Succeeded) return Results.BadRequest("Invalid email or password");

            return Results.Empty;
        });

        group.MapPost("/testuser", async (SignInManager<AppUser> signInManager, UserManager<AppUser> userManager) =>
        {
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

            var user = await userManager.FindByEmailAsync("test@test.com");
            if (user != null)
            {
                await signInManager.SignInAsync(user, false);
                return Results.Empty;
            }

            user = new AppUser
            {
                Email = "test@test.com",
                UserName = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                EmailConfirmed = true
            };

            var res = await userManager.CreateAsync(user, "Password1!");
            if (!res.Succeeded) return Results.BadRequest(res.Errors);

            await userManager.AddCustomClaimsAsync(user);
            await userManager.AddToRoleAsync(user, RoleNames.Admin);
            await signInManager.SignInAsync(user, false);

            return Results.Empty;
        });

        return app;
    }
}

record SignInUserRequest(string Email, string Password);