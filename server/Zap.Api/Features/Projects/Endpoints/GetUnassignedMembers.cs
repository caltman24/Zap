using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Projects.Filters;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class GetUnassignedCompanyMembers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/unassigned", Handle)
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
            .WithProjectCompanyValidation();

    public static async Task<Results<
        BadRequest<string>,
        ForbidHttpResult,
        Ok<SortedDictionary<string, List<MemberInfoDto>>>>>
        Handle(IProjectService projectService, CurrentUser currentUser, [FromRoute] string projectId)
    {
        var isPm = await projectService.ValidateProjectManagerAsync(projectId, currentUser.Member!.Id);
        if (!isPm && currentUser.Member!.Role.Name != RoleNames.Admin)
        {
            return TypedResults.Forbid();
        }

        // Filter company members not assigned to projectId
        var members = await projectService.GetUnassignedMembersAsync(projectId, currentUser.Member!.Id);
        if (members == null)
        {
            return TypedResults.BadRequest("Could not find company members.");
        }

        return TypedResults.Ok(members);
    }
}
