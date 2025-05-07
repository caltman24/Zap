using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class UpdateProject : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{projectId}", Handle)
        .Accepts<Request>("application/json")
        .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager);

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

    private static async Task<Results<NoContent, ForbidHttpResult, NotFound<string>>> Handle(
            [FromRoute] string projectId,
            Request updateProjectRequest,
            CurrentUser currentUser,
            IProjectService projectService)
    {
        var isPm = await projectService.ValidateProjectManagerAsync(projectId, currentUser.Member!.Id);
        if (!isPm)
        {
            return TypedResults.Forbid();
        }

        var success = await projectService.UpdateProjectByIdAsync(
                projectId,
                new UpdateProjectDto(
                    updateProjectRequest.Name,
                    updateProjectRequest.Description,
                    updateProjectRequest.Priority,
                    updateProjectRequest.DueDate));

        if (success) return TypedResults.NoContent();

        return TypedResults.NotFound("Failed to update company info.");
    }
}
