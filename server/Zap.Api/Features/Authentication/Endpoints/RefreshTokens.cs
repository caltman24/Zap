using FluentValidation;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Zap.Api.Common;
using Zap.Api.Common.Filters;
using Zap.Api.Data.Models;

namespace Zap.Api.Features.Authentication.Endpoints;

public class RefreshTokens : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/refresh", Handle)
            .AllowAnonymous()
            .WithRequestValidation<Request>();

    public record Request(string RefreshToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().NotNull();
        }
    }

    private static async Task<Results<ChallengeHttpResult, SignInHttpResult>> Handle(
        Request request,
        SignInManager<AppUser> signInManager,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider timeProvider,
        ILogger<Program> logger)
    {
        logger.LogDebug("Processing refresh token request");
        var refreshTokenProtector =
            bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(request.RefreshToken);

        //TODO:Figure out if the time expires or if the validation fails

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
