using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Endpoints;

public class GetUnassignedCompanyMembers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/unassigned", Handle);

    public static async Task<Results<
        BadRequest<string>,
        Ok<Dictionary<string, List<MemberInfoDto>>>>>
        Handle(IProjectService service, CurrentUser user, [FromRoute] string projectId)
    {
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

        // Get company members that arent apart of a given project
        // That excludes the person sending the request
        // That excludes any other member that is apart of the given project

        // TODO: Move logic to ProjectService
        // Filter company members not assigned to projectId
        var members = await service.GetUnassignedMembersAsync(projectId);
        if (members == null)
        {
            return TypedResults.BadRequest("Could not find company members.");
        }

        return TypedResults.Ok(members);
    }
}
