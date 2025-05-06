using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class ArchiveProject : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{projectId}/archive", Handle)
            .RequireAuthorization(pb =>
            {
                pb.RequireCurrentUser();
                pb.RequireCompanyRole(RoleNames.Admin, RoleNames.ProjectManager);
                pb.Build();
            });


    private static async Task<Results<BadRequest<string>, ForbidHttpResult, NoContent>> Handle(string projectId,
        IProjectService service, ICompanyService companyService, CurrentUser currentUser)
    {
        if (currentUser.Member == null)
        {
            return TypedResults.BadRequest("User not in company");
        }
        var isPm = await service.ValidateProjectManagerAsync(projectId, currentUser.Member.Id);
        if (!isPm)
        {
            return TypedResults.Forbid();
        }

        var success = await service.ToggleArchiveProjectAsync(projectId);
        if (!success) TypedResults.BadRequest("Project does not exist");

        return TypedResults.NoContent();
    }
}
