using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Authorization;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Services;

namespace Zap.Api.Endpoints;

internal static class ProjectsEndpoints
{
    internal static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects");
        group.MapGet("/{ProjectId}", GetProjectHandler)
            .WithName("GetProject");

        group.MapPost("/", CreateProjectHandler)
            .RequireAuthorization(pb =>
            {
                pb.RequireRole(RoleNames.Admin, RoleNames.ProjectManager)
                    .RequireCurrentUser()
                    .Build();
            });

        return app;
    }

    private static async Task<Results<BadRequest<string>, Ok<ProjectDto>>> GetProjectHandler(
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

    private static async Task<Results<BadRequest<string>, CreatedAtRoute<ProjectDto>>> CreateProjectHandler(
        CreateProjectRequest request, IProjectService projectService, CurrentUser currentUser, ILogger<Program> logger)
    {
        var user = currentUser.User;
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var newProject = await projectService.CreateProjectAsync(new CreateProjectDto
        (
            Name: request.Name,
            Description: request.Description,
            Priority: request.Priority,
            DueDate: request.DueDate,
            User: currentUser.User!
        ));

        return TypedResults.CreatedAtRoute(newProject, "GetProject", new { ProjectId = newProject.Id });
    }
}

record CreateProjectRequest(string Name, string Description, string Priority, DateTime DueDate);