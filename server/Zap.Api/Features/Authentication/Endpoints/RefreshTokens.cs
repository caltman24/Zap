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

        try
        {
            var refreshTokenProtector =
                bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;

            var refreshTicket = refreshTokenProtector.Unprotect(request.RefreshToken);

            if (refreshTicket == null)
            {
                logger.LogWarning("Refresh token could not be unprotected");
                return TypedResults.Challenge();
            }

            if (refreshTicket.Properties?.ExpiresUtc is not { } expiresUtc)
            {
                logger.LogWarning("Refresh ticket doesn't have an expiration time");
                return TypedResults.Challenge();
            }

            var currentTime = timeProvider.GetUtcNow();
            if (currentTime >= expiresUtc)
            {
                logger.LogWarning("Refresh token expired at {ExpiryTime}, current time is {CurrentTime}",
                    expiresUtc, currentTime);
                return TypedResults.Challenge();
            }

            var user = await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal);

            if (user == null)
            {
                logger.LogWarning("Security stamp validation failed for user");
                return TypedResults.Challenge();
            }

            logger.LogInformation("Refresh token validated successfully for user {Email}", user.Email);
            var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);

            return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing refresh token");
            return TypedResults.Challenge();
        }
    }
}
