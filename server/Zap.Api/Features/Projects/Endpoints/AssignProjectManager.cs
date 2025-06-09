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
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{projectId}/pm", Handle)
            .WithCompanyMember(RoleNames.Admin)
            .WithProjectCompanyValidation()
            .WithProjectArchiveValidation();

    public record Request(string? memberId);

    public static async Task<Results<BadRequest<string>, NoContent, NotFound<string>>> Handle(
            [FromRoute] string projectId,
            Request request,
            CurrentUser currentUser,
            IProjectService projectService,
            ICompanyService companyService)
    {
        // HACK:: When no memberId is present, remove the project manager. too lazy to make another endpoint
        if (request.memberId == null)
        {
            var success = await projectService.UpdateProjectManagerAsync(projectId, request.memberId);

            return success ? TypedResults.NoContent() : TypedResults.NotFound("Project not found");
        }

        var memberRole = await companyService.GetMemberRoleAsync(request.memberId);
        if (memberRole != RoleNames.ProjectManager)
        {
            return TypedResults.BadRequest("Assigned member is not a project manager");
        }

        var result = await projectService.UpdateProjectManagerAsync(projectId, request.memberId);

        return result ? TypedResults.NoContent() : TypedResults.NotFound("Project not found");

    }

}
