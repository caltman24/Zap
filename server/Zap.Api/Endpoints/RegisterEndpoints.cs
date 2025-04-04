using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Authorization;
using Zap.Api.Extensions;
using Zap.Api.Filters;
using Zap.DataAccess;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;
using Zap.DataAccess.Services;

namespace Zap.Api.Endpoints;

public static class RegisterEndpoints
{
    public static IEndpointRouteBuilder MapRegisterUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/register");

        group.MapPost("/user", RegisterUserHandler)
            .AllowAnonymous()
            .WithRequestValidation<RegisterUserRequest>();

        group.MapPost("/company", RegisterCompanyHandler)
            .WithRequestValidation<RegisterCompanyRequest>();

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

    private static async Task<Results<BadRequest<string>, NoContent>>
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

public record RegisterUserRequest(string Email, string Password, string FirstName, string LastName);

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty().NotNull();
        RuleFor(x => x.Password).NotEmpty().NotNull().MinimumLength(6);
        RuleFor(x => x.FirstName).NotNull();
        RuleFor(x => x.LastName).NotNull();
    }
}

public record RegisterCompanyRequest(string Name, string Description);

public class RegisterCompanyValidator : AbstractValidator<RegisterCompanyRequest>
{
    public RegisterCompanyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(75);
        RuleFor(x => x.Description).NotNull().MaximumLength(1000);
    }
}