using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetDeveloperList : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{ticketId}/developer-list", Handle)
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
            .WithTicketCompanyValidation();

    private static async Task<Ok<List<MemberInfoDto>>> Handle(
            [FromRoute] string ticketId,
            CurrentUser currentUser,
            ITicketService ticketService
            )
    {
        var projects = await ticketService.GetProjectDevelopersAsync(ticketId);

        return TypedResults.Ok(projects);
    }
}
