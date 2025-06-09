using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Data;
using Zap.Api.Features.Projects.Filters;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class UpdateProject : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{projectId}", Handle)
        .Accepts<Request>("application/json")
        .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
        .WithProjectCompanyValidation();

    public record Request(string Name, string Description, string Priority, DateTime DueDate);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().NotNull().MaximumLength(50);
            RuleFor(x => x.Description).NotEmpty().NotNull().MaximumLength(1000);
            RuleFor(x => x.Priority).NotEmpty().NotNull().MaximumLength(50);
            RuleFor(x => x.DueDate).NotEmpty().NotNull().GreaterThan(DateTime.UtcNow);
        }
    }

    private static async Task<Results<NoContent, ForbidHttpResult, NotFound<string>, BadRequest<string>>> Handle(
            [FromRoute] string projectId,
            Request updateProjectRequest,
            CurrentUser currentUser,
            IProjectService projectService,
            AppDbContext db)
    {
        var isPm = await projectService.ValidateProjectManagerAsync(projectId, currentUser.Member!.Id);
        if (!isPm && currentUser.Member!.Role.Name != RoleNames.Admin)
        {
            return TypedResults.Forbid();
        }

        // Check if project is archived
        var project = await db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => new { p.IsArchived, p.Name, p.Description, p.Priority, p.DueDate })
            .FirstOrDefaultAsync();

        if (project == null)
        {
            return TypedResults.NotFound("Project not found");
        }

        // If project is archived, only allow name and description updates
        if (project.IsArchived)
        {
            // Check if only name and description are being changed
            if (updateProjectRequest.Priority != project.Priority ||
                updateProjectRequest.DueDate != project.DueDate)
            {
                return TypedResults.BadRequest("Archived projects can only have their name and description updated.");
            }

            // Only update name and description for archived projects
            var success = await projectService.UpdateArchivedProjectAsync(
                projectId,
                updateProjectRequest.Name,
                updateProjectRequest.Description);

            if (success) return TypedResults.NoContent();
            return TypedResults.NotFound("Failed to update project.");
        }

        // For non-archived projects, allow full updates
        var fullUpdateSuccess = await projectService.UpdateProjectByIdAsync(
                projectId,
                new UpdateProjectDto(
                    updateProjectRequest.Name,
                    updateProjectRequest.Description,
                    updateProjectRequest.Priority,
                    updateProjectRequest.DueDate));

        if (fullUpdateSuccess) return TypedResults.NoContent();

        return TypedResults.NotFound("Failed to update project.");
    }
}
