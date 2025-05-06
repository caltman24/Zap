using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class GetAssignableProjectManagers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{projectId}/assignable-pms", Handle)
            .RequireAuthorization(pb => { pb.RequireCompanyMember(RoleNames.Admin); });

    private static async Task<Ok<List<MemberInfoDto>>> Handle(
            [FromRoute] string projectId,
            IProjectService projectService)
    {
        // INFO: If the project is not found, an empty list is returned.
        // If this was a public API, we would return a 404.
        var projectManagers = await projectService.GetAssignablePMs(projectId);
        return TypedResults.Ok(projectManagers);
    }

}
