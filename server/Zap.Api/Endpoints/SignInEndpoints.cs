using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Zap.Api.Extensions;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

internal static class SignInEndpoints
{
    internal static IEndpointRouteBuilder MapSignInEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/signin").AllowAnonymous();

        group.MapPost("/", SignInUserHandler);
        group.MapPost("/testuser", SignInTestUserHandler);

        app.MapPost("/refresh", RefreshTokenHandler).AllowAnonymous();

        return app;
    }


    private static async Task<Results<BadRequest<string>, EmptyHttpResult>> SignInUserHandler(SignInUserRequest request,
        SignInManager<AppUser> signInManager)
    {
        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
        if (!result.Succeeded) return TypedResults.BadRequest("Invalid email or password");

        return TypedResults.Empty;
    }

    private static async Task<Results<BadRequest<IEnumerable<IdentityError>>, SignInHttpResult>> SignInTestUserHandler(
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

    private static async Task<Results<ChallengeHttpResult, SignInHttpResult>> RefreshTokenHandler(
        RefreshTokenRequest request,
        SignInManager<AppUser> signInManager,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider timeProvider,
        ILogger<Program> logger)
    {
        logger.LogDebug("Processing refresh token request");
        var refreshTokenProtector =
            bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(request.RefreshToken);

        // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            timeProvider.GetUtcNow() >= expiresUtc ||
            await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user)
        {
            logger.LogDebug("Refresh token validation failed");
            return TypedResults.Challenge();
        }

        logger.LogInformation("Refresh token validated successfully for user {Email}", user.Email);
        var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);

        return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
    }
}

internal record SignInUserRequest(string Email, string Password);

internal record RefreshTokenRequest(string RefreshToken);