using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class RemoveMember : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{memberId}", Handle);

    public static async Task<Results<NotFound, NoContent>> Handle(
            [FromRoute] string projectId,
            [FromRoute] string memberId,
            IProjectService projectService)
    {
        var success = await projectService.RemoveMemberFromProjectAsync(projectId, memberId);
        if (!success) return TypedResults.NotFound();

        return TypedResults.NoContent();
    }

}

