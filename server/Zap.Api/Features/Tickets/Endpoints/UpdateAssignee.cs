using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class UpdateAssignee : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{ticketId}/developer", Handle)
            .WithName("UpdateDeveloper")
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager);

    private static async Task<Results<NotFound, Ok<BasicTicketDto>>> Handle(
            [FromRoute] string ticketId,
            ITicketService ticketService,
            CurrentUser currentUser
            )
    {
        // validate projectManager
        var ticket = await ticketService.GetTicketByIdAsync(ticketId);

        if (ticket == null) return TypedResults.NotFound();

        return TypedResults.Ok(ticket);
    }
}

