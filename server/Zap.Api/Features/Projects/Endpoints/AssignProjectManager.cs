using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Projects.Filters;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class AssignProjectManager : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{projectId}/pm", Handle)
            .WithCompanyMember(RoleNames.Admin)
            .WithProjectAccessValidation()
            .WithActiveProjectValidation();
    }

    public static async Task<Results<BadRequest<string>, NoContent, NotFound<string>>> Handle(
        [FromRoute] string projectId,
        Request request,
        CurrentUser currentUser,
        IProjectService projectService,
        ICompanyService companyService)
    {
        // HACK:: When no memberId is present, remove the project manager.
        if (request.MemberId == null)
        {
            var success = await projectService.UpdateProjectManagerAsync(projectId, request.MemberId);

            return success ? TypedResults.NoContent() : TypedResults.NotFound("Project not found");
        }

        var memberInSameProjectCompany = await projectService
            .AreMembersInProjectCompanyAsync(projectId, [request.MemberId]);

        if (!memberInSameProjectCompany)
            return TypedResults.BadRequest("Member is not in the same company");

        var memberRole = await companyService.GetMemberRoleAsync(request.MemberId);
        if (memberRole != RoleNames.ProjectManager)
            return TypedResults.BadRequest("Member is not a project manager");


        var result = await projectService.UpdateProjectManagerAsync(projectId, request.MemberId);

        return result ? TypedResults.NoContent() : TypedResults.NotFound("Project not found");
    }

    public record Request(string? MemberId);
}