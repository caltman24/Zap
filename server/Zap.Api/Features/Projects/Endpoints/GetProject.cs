using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Projects.Filters;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class GetProject : IEndpoint
{

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{projectId}", Handle)
           .WithName("GetProject")
           .WithCompanyMember()
           .WithProjectCompanyValidation();

    private static async Task<Results<NotFound<string>, Ok<ProjectDto>>> Handle(
        [FromRoute] string projectId, IProjectService projectService, CurrentUser currentUser, ILogger<Program> logger)
    {
        var project = await projectService.GetProjectByIdAsync(projectId);
        if (project == null) return TypedResults.NotFound("Project not found");

        // If the project is not in the same company as the user
        if (project.CompanyId != currentUser.Member!.CompanyId) return TypedResults.NotFound("Project not found");

        return TypedResults.Ok(project);
    }

}
