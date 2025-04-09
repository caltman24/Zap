using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class ArchiveProject : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{projectId}/archive", Handle)
            .RequireAuthorization(pb =>
            {
                pb.RequireRole(RoleNames.Admin, RoleNames.ProjectManager);
                pb.RequireCurrentUser();
                pb.Build();
            });


    private static async Task<Results<BadRequest<string>, NoContent>> Handle(string projectId,
        IProjectService service)
    {
        var success = await service.ToggleArchiveProjectAsync(projectId);
        if (!success) TypedResults.BadRequest("Project does not exist");

        return TypedResults.NoContent();
    }
}