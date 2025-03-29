using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Authorization;
using Zap.Api.Extensions;
using Zap.DataAccess;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

internal static class RegisterEndpoints
{
    internal static IEndpointRouteBuilder MapRegisterUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/register");

        // verify user status
        group.MapPost("/verify",
            Results<BadRequest<string>, Ok<RegisterVerifyResponse>> (CurrentUser currentUser) => TypedResults.Ok(
                currentUser.CompanyId != null
                    ? new RegisterVerifyResponse("company")
                    : new RegisterVerifyResponse("none")));

        group.MapPost("/user", RegisterUserHandler).AllowAnonymous();

        group.MapPost("/company", RegisterCompanyHandler);


        return app;
    }


    private static async Task<Results<BadRequest<string>, BadRequest<IEnumerable<IdentityError>>, SignInHttpResult>>
        RegisterUserHandler(
            RegisterUserRequest request, UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, ILogger<Program> logger)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user != null) return TypedResults.BadRequest("An account is already registered with this email");

        // TODO: Add email confirmation

        var newUser = new AppUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };
        newUser.SetDefaultAvatar();

        var result = await userManager.CreateAsync(newUser, request.Password);
        if (!result.Succeeded) return TypedResults.BadRequest(result.Errors);
        logger.LogDebug("Created new user {Email} with default avatar {AvatarUrl}", newUser.Email,
            newUser.AvatarUrl);
        logger.LogInformation("New user registered: {Email}", newUser.Email);

        await userManager.AddCustomClaimsAsync(newUser);
        logger.LogDebug("Added custom claims to user {Email}", newUser.Email);

        var principal = await signInManager.CreateUserPrincipalAsync(newUser);

        return TypedResults.SignIn(principal, authenticationScheme: IdentityConstants.BearerScheme);
    }

    private static async Task<Results<BadRequest<string>, InternalServerError, Ok<RegisterCompanyResponse>>>
        RegisterCompanyHandler(
            RegisterCompanyRequest request, AppDbContext db, CurrentUser currentUser,
            HttpContext context, ILogger<Program> logger, UserManager<AppUser> userManager)
    {
        if (currentUser.User == null) return TypedResults.BadRequest("User not found");

        if (currentUser.CompanyId != null)
        {
            return TypedResults.BadRequest("User already exists in a company");
        }

        var newCompany = new Company
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = currentUser.Id,
            Members = new List<AppUser> { currentUser.User! },
        };

        await db.Companies.AddAsync(newCompany);
        await db.SaveChangesAsync();

        logger.LogInformation("User {Email} registered new company {Name}", currentUser.Email, newCompany.Name);

        await userManager.AddToRoleAsync(currentUser.User, RoleNames.Admin);
        logger.LogDebug("Added user {Email} to role {Role}", currentUser.Email, RoleNames.Admin);

        return TypedResults.Ok(new RegisterCompanyResponse(newCompany.Id, newCompany.Name,
            newCompany.Description));
    }
}

record RegisterVerifyResponse(string Result);

record RegisterUserRequest(string Email, string Password, string FirstName, string LastName);

record RegisterCompanyRequest(string Name, string Description);

record RegisterCompanyResponse(string Id, string Name, string Description);