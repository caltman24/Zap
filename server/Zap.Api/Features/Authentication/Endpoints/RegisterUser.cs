using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Zap.Api.Common;
using Zap.Api.Common.Extensions;
using Zap.Api.Common.Filters;
using Zap.Api.Data.Models;

namespace Zap.Api.Features.Authentication.Endpoints;

public class RegisterUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/register", Handle)
            .WithRequestValidation<Request>();
    }


    private static async Task<Results<BadRequest<string>, BadRequest<IEnumerable<IdentityError>>, SignInHttpResult>>
        Handle(
            Request request, UserManager<AppUser> userManager,
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

    public record Request(string Email, string Password, string FirstName, string LastName);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).EmailAddress().NotEmpty().NotNull();
            RuleFor(x => x.Password).NotEmpty().NotNull().MinimumLength(6);
            RuleFor(x => x.FirstName).NotNull();
            RuleFor(x => x.LastName).NotNull();
        }
    }
}