using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{ticketId}", Handle)
            .WithName("GetTicket")
            .WithCompanyMember();

    private static async Task<Results<NotFound, Ok<BasicTicketDto>>> Handle(
            [FromRoute] string ticketId,
            ITicketService ticketService,
            CurrentUser currentUser
            )
    {
        var sameCompany = await ticketService.ValidateCompanyAsync(ticketId, currentUser.Member!.CompanyId);
        var ticket = await ticketService.GetTicketByIdAsync(ticketId);

        if (ticket == null) return TypedResults.NotFound();

        return TypedResults.Ok(ticket);
    }
}
