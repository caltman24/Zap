using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Companies.Endpoints;

public class GetCompanyProjects : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", Handle)
            .WithCompanyMember();
    }

    private static async Task<Results<BadRequest<string>, Ok<List<CompanyProjectDto>>>> Handle(
        ICompanyService companyService, CurrentUser currentUser, ILogger<Program> logger,
        [FromQuery] bool? isArchived = null)
    {
        var projects = isArchived == null
            ? await companyService.GetAllCompanyProjectsAsync(currentUser.CompanyId!)
            : await companyService.GetCompanyProjectsAsync(currentUser.CompanyId!, isArchived.Value);

        return TypedResults.Ok(projects);
    }
}