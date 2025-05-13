using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetOpenTickets : IEndpoint
{
    // FIXME: Figure out role permissions here
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/open", Handle)
            .WithName("OpenTickets")
            .WithCompanyMember();

    private static async Task<Ok<List<BasicTicketDto>>> Handle(
            ITicketService ticketService,
            CurrentUser currentUser
            )
    {
        var tickets = await ticketService.GetOpenTicketsAsync(currentUser.CompanyId!);

        return TypedResults.Ok(tickets);
    }
}
