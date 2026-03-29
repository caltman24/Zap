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
            .WithProjectAccessValidation()
            .WithActiveProjectValidation();
    }

    public static async Task<Results<BadRequest<string>, ForbidHttpResult, NoContent>> Handle(
        [FromRoute] string projectId,
        [FromBody] Request request,
        CurrentUser currentUser,
        IProjectService projectService)
    {
        var isPm = await projectService.ValidateProjectManagerAsync(projectId, currentUser.Member!.Id);
        if (!isPm && currentUser.Member!.Role.Name != RoleNames.Admin) return TypedResults.Forbid();

        // TODO: Maybe put this inside the mutation method instead. Use a enum that named AddMembersResult, return an enum
        // from the mutation method and use that to determine the Result response to send.
        var sameProjectCompany = await projectService.AreMembersInProjectCompanyAsync(projectId, request.MemberIds);
        if (!sameProjectCompany) return TypedResults.BadRequest("Members are not in the same company as project");

        var success = await projectService.AddMembersToProjectAsync(projectId, request.MemberIds);
        if (success) return TypedResults.NoContent();

        return TypedResults.BadRequest("Failed to add members to project");
    }

    public record Request(IEnumerable<string> MemberIds);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.MemberIds).NotEmpty();
        }
    }
}
