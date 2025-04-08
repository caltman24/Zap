using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class GetProject : IEndpoint
{

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{projectId}", Handle)
            .WithName("GetProject");
    
    private static async Task<Results<BadRequest<string>, Ok<ProjectDto>>> Handle(
        [FromRoute] string projectId, IProjectService projectService, CurrentUser currentUser, ILogger<Program> logger)
    {
        var user = currentUser.User;
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var project = await projectService.GetProjectByIdAsync(projectId);
        if (project == null) return TypedResults.BadRequest("Project not found");

        // If the project is not in the same company as the user
        if (project.CompanyId != user.CompanyId) return TypedResults.BadRequest("Project not found");

        return TypedResults.Ok(project);
    }

}