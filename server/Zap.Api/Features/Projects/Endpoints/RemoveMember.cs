using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Projects.Filters;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class RemoveMember : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{memberId}", Handle)
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
            .WithProjectCompanyValidation();

    public static async Task<Results<NotFound, ForbidHttpResult, NoContent>> Handle(
            [FromRoute] string projectId,
            [FromRoute] string memberId,
            CurrentUser currentUser,
            IProjectService projectService)
    {
        var isPm = await projectService.ValidateProjectManagerAsync(projectId, currentUser.Member!.Id);
        if (!isPm && currentUser.Member!.Role.Name != RoleNames.Admin)
        {
            return TypedResults.Forbid();
        }

        var success = await projectService.RemoveMemberFromProjectAsync(projectId, memberId);
        if (!success) return TypedResults.NotFound();

        return TypedResults.NoContent();
    }

}
