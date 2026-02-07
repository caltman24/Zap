using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Filters;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Companies.Endpoints;

public class RegisterCompany : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapPost("/register", Handle)
            .WithRequestValidation<Request>();
    }

    private static async Task<Results<BadRequest<string>, NoContent>>
        Handle(
            Request request, ICompanyService companyService, CurrentUser currentUser,
            HttpContext context, ILogger<Program> logger, UserManager<AppUser> userManager)
    {
        if (currentUser.User == null) return TypedResults.BadRequest("User not found");

        if (currentUser.CompanyId != null) return TypedResults.BadRequest("User is already a member in a company");

        await companyService.CreateCompanyAsync(new CreateCompanyDto(
            request.Name,
            request.Description,
            currentUser.User));

        logger.LogDebug("Added user {Email} to role {Role}", currentUser.Email, RoleNames.Admin);

        return TypedResults.NoContent();
    }

    public record Request(string Name, string Description);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(75);
            RuleFor(x => x.Description).NotNull().MaximumLength(1000);
        }
    }

    public record Response(string CompanyId);
}