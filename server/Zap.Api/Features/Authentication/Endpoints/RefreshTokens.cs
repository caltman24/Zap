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
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/refresh", Handle)
            .AllowAnonymous()
            .WithRequestValidation<Request>();
    }

    private static async Task<Results<ChallengeHttpResult, SignInHttpResult>> Handle(
        Request request,
        SignInManager<AppUser> signInManager,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider timeProvider,
        ILogger<RefreshTokens> logger)
    {
        try
        {
            var refreshTokenProtector =
                bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;

            var refreshTicket = refreshTokenProtector.Unprotect(request.RefreshToken);

            if (refreshTicket == null)
                return TypedResults.Challenge();

            if (refreshTicket.Properties?.ExpiresUtc is not { } expiresUtc)
                return TypedResults.Challenge();

            var currentTime = timeProvider.GetUtcNow();
            if (currentTime >= expiresUtc)
                return TypedResults.Challenge();

            var user = await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal);

            if (user == null)
                return TypedResults.Challenge();

            var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);

            return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing refresh token");
            return TypedResults.Challenge();
        }
    }

    public record Request(string RefreshToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().NotNull();
        }
    }
}
