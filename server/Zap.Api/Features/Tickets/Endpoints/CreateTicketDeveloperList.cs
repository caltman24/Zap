using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Tickets;

public class CreateTicketDeveloperList : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/developer-list", Handle)
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager);

    private static async Task<Ok<List<BasicProjectDto>>> Handle(
            CurrentUser currentUser,
            IProjectService projectService
            )
    {
        // TODO: Replace with get developer list logic
        var projects = await projectService.GetAssignedProjects(
                currentUser.Member!.Id,
                currentUser.Role,
                currentUser.CompanyId!);

        return TypedResults.Ok(projects);
    }
}
