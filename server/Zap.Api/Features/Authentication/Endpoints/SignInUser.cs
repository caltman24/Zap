using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Zap.Api.Common;
using Zap.Api.Common.Filters;
using Zap.Api.Data.Models;

namespace Zap.Api.Features.Authentication.Endpoints;

public class SignInUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/signin", Handle)
            .WithRequestValidation<Request>();
    }

    private static async Task<Results<BadRequest<string>, EmptyHttpResult>> Handle(Request request,
        SignInManager<AppUser> signInManager)
    {
        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
        if (!result.Succeeded) return TypedResults.BadRequest("Invalid email or password");

        return TypedResults.Empty;
    }

    public record Request(string Email, string Password);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Email).EmailAddress();
            RuleFor(r => r.Password).NotEmpty();
        }
    }
}