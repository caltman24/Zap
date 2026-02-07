using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetMyTickets : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/mytickets", Handle)
            .WithName("GetMyTicket")
            .WithCompanyMember();
    }

    private static async Task<Ok<List<BasicTicketDto>>> Handle(
        ITicketService ticketService,
        CurrentUser currentUser
    )
    {
        var tickets = await ticketService.GetAssignedTicketsAsync(currentUser.Member!.Id);

        return TypedResults.Ok(tickets);
    }
}