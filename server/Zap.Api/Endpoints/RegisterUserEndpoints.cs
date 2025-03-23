using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Extensions;
using Zap.DataAccess;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

public static class RegisterUserEndpoints
{
    public static IEndpointRouteBuilder MapRegisterUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/register");

        // verify user status
        group.MapPost("/verify", async (UserManager<AppUser> userManager, HttpContext context) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            // user exists and is already in a company
            return Results.Ok(user?.CompanyId != null
                ? new RegisterVerifyResponse("company")
                : new RegisterVerifyResponse("none"));
        }).RequireAuthorization();

        group.MapPost("/user",
            async (RegisterUserRequest request, UserManager<AppUser> userManager,
                SignInManager<AppUser> signInManager) =>
            {
                var user = await userManager.FindByEmailAsync(request.Email);
                if (user != null) return Results.BadRequest("An account is already registered with this email");

                // TODO: Add email confirmation

                var newUser = new AppUser
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, request.Password);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);

                await userManager.AddCustomClaimsAsync(newUser);

                signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
                await signInManager.PasswordSignInAsync(newUser, request.Password, false, false);

                return Results.Empty;
            });

        // register company
        group.MapPost("/company",
            async (RegisterCompanyRequest request, AppDbContext db, UserManager<AppUser> userManager,
                HttpContext context) =>
            {
                var user = await userManager.FindByEmailAsync(context.User.FindFirstValue(ClaimTypes.Email)!);
                if (user == null) return Results.InternalServerError();

                if (user.CompanyId != null)
                {
                    return Results.BadRequest("User already exists in a company");
                }

                var newCompany = new Company
                {
                    Name = request.Name,
                    Description = request.Description,
                    OwnerId = user.Id,
                    Members = new List<AppUser> { user },
                };

                await db.Companies.AddAsync(newCompany);
                await db.SaveChangesAsync();

                await userManager.AddToRoleAsync(user, "Admin");

                return Results.Ok(new RegisterCompanyResponse(newCompany.Id, newCompany.Name, newCompany.Description));
            }).RequireAuthorization();


        return app;
    }
}

record RegisterVerifyResponse(string Result);

record RegisterUserRequest(string Email, string Password, string FirstName, string LastName);

record RegisterCompanyRequest(string Name, string Description);

record RegisterCompanyResponse(string Id, string Name, string Description);
