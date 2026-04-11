using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Projects.Filters;
using Zap.Api.Features.Projects.Services;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class GetProject : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{projectId}", Handle)
            .WithName("GetProject")
            .WithCompanyMember()
            .WithProjectAccessValidation();
    }

    private static async Task<Results<NotFound<string>, Ok<Response>>> Handle(
        [FromRoute] string projectId,
        IProjectService projectService,
        IProjectAuthorizationService projectAuthorizationService,
        CurrentUser currentUser,
        ILogger<Program> logger)
    {
        var project = await projectService.GetProjectByIdAsync(projectId);
        if (project == null) return TypedResults.NotFound("Project not found");

        // If the project is not in the same company as the user
        if (project.CompanyId != currentUser.Member!.CompanyId) return TypedResults.NotFound("Project not found");

        var capabilities = projectAuthorizationService.GetCapabilities(project, currentUser);

        return TypedResults.Ok(new Response(
            project.Id,
            project.Name,
            project.Description,
            project.Priority,
            project.CompanyId,
            project.ProjectManager,
            project.IsArchived,
            project.DueDate,
            project.Tickets,
            project.Members,
            capabilities));
    }

    private record Response(
        string Id,
        string Name,
        string Description,
        string Priority,
        string CompanyId,
        MemberInfoDto? ProjectManager,
        bool IsArchived,
        DateTime DueDate,
        IEnumerable<BasicTicketDto> Tickets,
        IEnumerable<MemberInfoDto> Members,
        ProjectCapabilitiesDto Capabilities);
}