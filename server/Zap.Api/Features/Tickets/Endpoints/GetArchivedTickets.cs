using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetArchivedTickets : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/archived", Handle)
            .WithName("ArchivedTickets")
            .WithCompanyMember();

    private static async Task<Ok<List<BasicTicketDto>>> Handle(
            ITicketService ticketService,
            CurrentUser currentUser
            )
    {
        var tickets = await ticketService.GetArchivedTicketsAsync(currentUser.CompanyId!);

        return TypedResults.Ok(tickets);
    }
}
