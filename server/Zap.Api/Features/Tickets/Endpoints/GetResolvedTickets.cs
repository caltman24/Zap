using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetResolvedTickets : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/resolved", Handle)
            .WithName("ResolvedTickets")
            .WithCompanyMember();
    }

    private static async Task<Ok<List<BasicTicketDto>>> Handle(
        ITicketService ticketService,
        CurrentUser currentUser
    )
    {
        var tickets = await ticketService.GetResolvedTicketsAsync(currentUser.CompanyId!);

        return TypedResults.Ok(tickets);
    }
}