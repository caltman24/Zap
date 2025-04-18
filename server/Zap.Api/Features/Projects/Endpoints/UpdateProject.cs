﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class UpdateProject : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{projectId}", Handle)
        .Accepts<Request>("application/json")
        .RequireAuthorization(pb =>
                {
                    pb.RequireRole([RoleNames.Admin, RoleNames.ProjectManager]);
                });

    public record Request(string Name, string Description, string Priority, DateTime DueDate);

    private static async Task<Results<NoContent, BadRequest<string>>> Handle(
            [FromRoute] string projectId,
            Request updateProjectRequest,
            CurrentUser currentUser,
            IProjectService projectService)
    {

        var success = await projectService.UpdateProjectByIdAsync(
                projectId,
                new UpdateProjectDto(
                    updateProjectRequest.Name,
                    updateProjectRequest.Description,
                    updateProjectRequest.Priority,
                    updateProjectRequest.DueDate));

        if (success) return TypedResults.NoContent();

        return TypedResults.BadRequest("Failed to update company info.");
    }
}
