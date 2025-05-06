
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class UpdateProjectManager : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{projectId}/pm", Handle)
        .RequireAuthorization(pb =>
                {
                    pb.RequireCurrentUser();
                    pb.RequireCompanyMember(RoleNames.Admin);
                    pb.Build();
                });

    public record Request(string memberId);

    public static async Task<Results<BadRequest<string>, NoContent>> Handle(
            Request request,
            CurrentUser currentUser,
            IProjectService projectService,
            ICompanyService companyService,
            [FromRoute] string projectId)
    {
        if (currentUser.Member == null)
        {
            return TypedResults.BadRequest("User not in company");
        }

        var memberRole = await companyService.GetMemberRoleAsync(request.memberId);
        if (memberRole != RoleNames.ProjectManager)
        {
            return TypedResults.BadRequest("Assigned member is not a project manager");
        }
        var success = await projectService.UpdateProjectManagerAsync(projectId, request.memberId);

        return success ? TypedResults.NoContent() : TypedResults.BadRequest("Project not found");

    }

}
