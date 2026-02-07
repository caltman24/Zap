using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Filters;
using Zap.Api.Features.Projects.Filters;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class AddMembers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/", Handle)
            .Accepts<Request>("application/json")
            .WithRequestValidation<Request>()
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
            .WithProjectCompanyValidation()
            .WithProjectArchiveValidation();
    }

    public static async Task<Results<BadRequest<string>, ForbidHttpResult, NoContent>> Handle(
        [FromRoute] string projectId,
        [FromBody] Request request,
        CurrentUser currentUser,
        IProjectService projectService)
    {
        var isPm = await projectService.ValidateProjectManagerAsync(projectId, currentUser.Member!.Id);
        if (!isPm && currentUser.Member!.Role.Name != RoleNames.Admin) return TypedResults.Forbid();

        var success = await projectService.AddMembersToProjectAsync(projectId, request.memberIds);
        if (success) return TypedResults.NoContent();

        return TypedResults.BadRequest("Failed to add members to project");
    }

    public record Request(IEnumerable<string> memberIds);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.memberIds).NotEmpty();
        }
    }
}