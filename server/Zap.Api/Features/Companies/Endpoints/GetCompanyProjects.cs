using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Companies.Endpoints;

public class GetCompanyProjects : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/", Handle);

    private static async Task<Results<BadRequest<string>, Ok<List<CompanyProjectDto>>>> Handle(
        ICompanyService companyService, CurrentUser currentUser, ILogger<Program> logger,
        [FromQuery] bool? isArchived = null)
    {
        var user = currentUser.User;
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var projects = isArchived == null
            ? await companyService.GetAllCompanyProjectsAsync(user.CompanyId)
            : await companyService.GetCompanyProjectsAsync(user.CompanyId, isArchived.Value);

        return TypedResults.Ok(projects);
    }
}