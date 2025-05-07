using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class AssignProjectManager : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{projectId}/assign-pm", Handle)
            .WithCompanyMember(RoleNames.Admin);

    public record Request(string MemberId);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.MemberId).NotEmpty().NotNull();
        }
    }

    private static async Task<Results<NotFound<string>, BadRequest<string>, NoContent>> Handle(
            [FromRoute] string projectId,
            Request request,
            IProjectService projectService,
            ICompanyService companyService
            )
    {
        var memberRole = await companyService.GetMemberRoleAsync(request.MemberId);
        if (memberRole == null) return TypedResults.NotFound("Member with memberId does not exist");
        if (memberRole != RoleNames.ProjectManager) return TypedResults.BadRequest("Member with memberId is not a project manager");

        var result = await projectService.UpdateProjectManagerAsync(projectId, request.MemberId);
        if (!result) return TypedResults.NotFound("Project not found");

        return TypedResults.NoContent();
    }
}
