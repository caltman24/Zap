using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class DeleteTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{ticketId}", Handle)
            .WithName("DeleteTicket")
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager)
            // This endpoint filter checks the ticketId parm, to validate if the requesting user is in the same company
            // as the ticket. Only works on endpoints with exisiting ticket id
            .WithTicketCompanyValidation();

    private static async Task<NoContent> Handle(
            [FromRoute] string ticketId,
            ITicketService ticketService
            )
    {
        await ticketService.DeleteTicketAsync(ticketId);

        return TypedResults.NoContent();
    }
}
