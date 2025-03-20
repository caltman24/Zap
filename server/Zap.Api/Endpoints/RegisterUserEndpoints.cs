using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Extensions;
using Zap.DataAccess;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

public static class RegisterUserEndpoints
{
    public static IEndpointRouteBuilder MapRegisterUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/register");

        // verify user status
        group.MapPost("/verify", async ([FromQuery] string email, UserManager<AppUser> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(email);
            // user exists and is already in a company
            if (user?.CompanyId != null)
            {
                return Results.Ok(new RegisterVerifyResponse("company"));
            }

            // user exists and not in a company
            if (user != null && user.CompanyId == null)
            {
                return Results.Ok(new RegisterVerifyResponse("user"));
            }

            return Results.Ok(new RegisterVerifyResponse("none"));
        });

        // register company
        group.MapPost("/company",
            async (RegisterCompanyRequest request, AppDbContext db, UserManager<AppUser> userManager,
                SignInManager<AppUser> signInManager) =>
            {
                var user = await userManager.FindByEmailAsync(request.Email);
                if (user?.CompanyId != null)
                {
                    return Results.BadRequest("User already exists in a company");
                }

                if (user == null) return Results.BadRequest("User does not exist by email");

                var newCompany = new Company
                {
                    Name = request.Name,
                    Description = request.Description,
                    Owner = user,
                    Members = new List<AppUser> { user },
                };
                await db.Companies.AddAsync(newCompany);
                await db.SaveChangesAsync();
                
                await userManager.AddToRoleAsync(user, "Admin");

                signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
                await signInManager.SignInAsync(user, false);

                return Results.Empty;
            });

        // Register user and company
        group.MapPost("/company-user",
            async (RegisterCompanyUserRequest request, AppDbContext db, UserManager<AppUser> userManager,
                SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager) =>
            {
                var user = await userManager.FindByEmailAsync(request.Owner.Email);
                if (user != null) return Results.BadRequest("User already exists");

                var newUser = new AppUser
                {
                    Email = request.Owner.Email,
                    UserName = request.Owner.Email,
                    FirstName = request.Owner.FirstName,
                    LastName = request.Owner.LastName,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(newUser, request.Owner.Password);
                if (!result.Succeeded) return Results.BadRequest(result.Errors);


                var roleExists = await roleManager.RoleExistsAsync("Admin");
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                await userManager.AddToRoleAsync(newUser, "Admin");
                await userManager.AddCustomClaimsAsync(newUser);

                var company = new Company
                {
                    Name = request.Name,
                    Description = request.Description,
                    Owner = newUser,
                    Members = new List<AppUser> { newUser },
                };
                await db.Companies.AddAsync(company);
                await db.SaveChangesAsync();

                signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
                var res = await signInManager.PasswordSignInAsync(newUser, request.Owner.Password, false, false);
                if (!res.Succeeded) return Results.BadRequest("Invalid email or password");

                return Results.Empty;
            });

        return app;
    }
}