using Microsoft.AspNetCore.Identity;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

public static class SignInEndpoints
{
    public static IEndpointRouteBuilder MapSignInEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/signin");

        group.MapPost("/company", async (SignInCompanyRequest request, SignInManager<AppUser> signInManager) =>
        {
            var user = await signInManager.UserManager.FindByEmailAsync(request.Email);
            if (user == null) return Results.BadRequest("Invalid email or password");
            
            var validPassword = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!validPassword.Succeeded) return Results.BadRequest("Invalid email or password");

            if (user.CompanyId == null)
            {
                return Results.BadRequest("User is not in a company. Register a company or join via invite.");
            }
            
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
            var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
            if (!result.Succeeded) return Results.BadRequest("Invalid email or password");
            
            return Results.Empty;
        });

        return app;
    }
}

record SignInCompanyRequest(string Email, string Password);