using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Zap.Api.Common;
using Zap.Api.Common.Filters;
using Zap.Api.Data.Models;
using Zap.Api.Features.Demo;
using Zap.Api.Features.Demo.Services;

namespace Zap.Api.Features.Authentication.Endpoints;

public class SignInDemoUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/signin-demo", Handle)
            .WithRequestValidation<Request>();
    }

    private static async Task<Results<BadRequest<string>, SignInHttpResult>> Handle(
        Request request,
        IDemoEnvironmentService demoEnvironmentService,
        SignInManager<AppUser> signInManager)
    {
        var user = await demoEnvironmentService.GetDemoUserByRoleAsync(request.Role);
        if (user == null) return TypedResults.BadRequest("Invalid demo role");

        var principal = await signInManager.CreateUserPrincipalAsync(user);

        return TypedResults.SignIn(principal, authenticationScheme: IdentityConstants.BearerScheme);
    }

    public record Request(string Role);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty()
                .Must(role => DemoRoleKeys.ToList().Contains(role));
        }
    }
}