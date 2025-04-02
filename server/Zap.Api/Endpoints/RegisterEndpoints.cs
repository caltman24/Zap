using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Authorization;
using Zap.Api.Extensions;
using Zap.DataAccess;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;
using Zap.DataAccess.Services;

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

    private static async Task<Results<BadRequest<string>, InternalServerError, NoContent>>
        RegisterCompanyHandler(
            RegisterCompanyRequest request, ICompanyService companyService, CurrentUser currentUser,
            HttpContext context, ILogger<Program> logger, UserManager<AppUser> userManager)
    {
        if (currentUser.User == null) return TypedResults.BadRequest("User not found");

        if (currentUser.CompanyId != null)
        {
            return TypedResults.BadRequest("User already exists in a company");
        }

        await companyService.CreateCompanyAsync(new CreateCompanyDto(
            Name: request.Name,
            Description: request.Description,
            User: currentUser.User));

        await userManager.AddToRoleAsync(currentUser.User, RoleNames.Admin);
        logger.LogDebug("Added user {Email} to role {Role}", currentUser.Email, RoleNames.Admin);

        return TypedResults.NoContent();
    }
}

internal record RegisterVerifyResponse(string Result);

internal record RegisterUserRequest(string Email, string Password, string FirstName, string LastName);

internal record RegisterCompanyRequest(string Name, string Description);
