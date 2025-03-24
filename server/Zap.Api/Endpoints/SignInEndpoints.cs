using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Zap.Api.Extensions;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

public static class SignInEndpoints
{
    public static IEndpointRouteBuilder MapSignInEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/signin");

        group.MapPost("/",
            async Task<Results<BadRequest<string>, EmptyHttpResult>> (SignInUserRequest request,
                SignInManager<AppUser> signInManager) =>
            {
                signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
                var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
                if (!result.Succeeded) return TypedResults.BadRequest("Invalid email or password");

                return TypedResults.Empty;
            });

        group.MapPost("/testuser",
            async Task<Results<BadRequest<IEnumerable<IdentityError>>, EmptyHttpResult>> (
                SignInManager<AppUser> signInManager, UserManager<AppUser> userManager) =>
            {
                signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

                var user = await userManager.FindByEmailAsync("test@test.com");
                if (user != null)
                {
                    await signInManager.SignInAsync(user, false);
                    return TypedResults.Empty;
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
                if (!res.Succeeded) return TypedResults.BadRequest(res.Errors);

                await userManager.AddCustomClaimsAsync(user);
                await userManager.AddToRoleAsync(user, RoleNames.Admin);
                await signInManager.SignInAsync(user, false);

                return TypedResults.Empty;
            });


        app.MapPost("/refresh",
            async Task<Results<ChallengeHttpResult, SignInHttpResult>> (RefreshTokenRequest request,
                SignInManager<AppUser> signInManager,
                IOptionsMonitor<BearerTokenOptions> bearerTokenOptions, TimeProvider timeProvider) =>
            {
                var refreshTokenProtector =
                    bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
                var refreshTicket = refreshTokenProtector.Unprotect(request.RefreshToken);

                // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
                if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
                    timeProvider.GetUtcNow() >= expiresUtc ||
                    await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user)

                {
                    return TypedResults.Challenge();
                }

                var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);

                return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
            });


        return app;
    }
}

record SignInUserRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);