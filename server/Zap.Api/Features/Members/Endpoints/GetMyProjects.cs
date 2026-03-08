using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Members.Endpoints;

public class GetMyProjects : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{memberId}/myprojects", Handle)
            .WithCompanyMember(RoleNames.Submitter, RoleNames.Developer, RoleNames.ProjectManager);
    }

    private static async Task<Results<Ok<List<CompanyProjectDto>>, NotFound>> Handle(
        ICompanyService companyService,
        CurrentUser currentUser
    )
    {
        var projects = await companyService.GetVisibleProjectsAsync(
            currentUser.CompanyId!,
            currentUser.Member!.Id,
            currentUser.Member.Role.Name,
            false);

        return TypedResults.Ok(projects);
    }
}
