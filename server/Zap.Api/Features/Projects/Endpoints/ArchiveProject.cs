using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Projects.Filters;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class ArchiveProject : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{projectId}/archive", Handle)
        .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
        .WithProjectCompanyValidation();

    private static async Task<Results<NotFound<string>, ForbidHttpResult, NoContent>> Handle(string projectId,
        IProjectService service, ICompanyService companyService, CurrentUser currentUser)
    {
        var isPm = await service.ValidateProjectManagerAsync(projectId, currentUser.Member!.Id);
        if (!isPm && currentUser.Member!.Role.Name != RoleNames.Admin)
        {
            return TypedResults.Forbid();
        }

        var success = await service.ToggleArchiveProjectAsync(projectId);
        if (!success) TypedResults.NotFound("Project not found");

        return TypedResults.NoContent();
    }
}
