using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Companies.Services;

namespace Zap.Api.Companies.Endpoints;

public class GetCompanyProjects : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/", Handle);
    
    private static async Task<Results<BadRequest<string>, Ok<List<CompanyProjectDto>>>> Handle(
        ICompanyService companyService, CurrentUser currentUser, ILogger<Program> logger, [FromQuery] bool isArchived)
    {
        var user = currentUser.User;
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var projects = await companyService.GetCompanyProjectsAsync(user.CompanyId, isArchived);

        return TypedResults.Ok(projects);
    }
    
}