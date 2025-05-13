using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Tickets;

public class CreateTicketProjectList : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/project-list", Handle)
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager, RoleNames.Submitter);

    private static async Task<Ok<List<BasicProjectDto>>> Handle(
            CurrentUser currentUser,
            IProjectService projectService
            )
    {
        var projects = await projectService.GetAssignedProjects(
                currentUser.Member!.Id,
                currentUser.Member!.Role.Name,
                currentUser.CompanyId!);

        return TypedResults.Ok(projects);
    }
}
