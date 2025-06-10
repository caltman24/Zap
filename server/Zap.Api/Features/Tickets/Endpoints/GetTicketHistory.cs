using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetTicketHistory : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{ticketId}/history", Handle)
            .WithName("GetTicketHistory")
            .WithCompanyMember()
            .WithTicketCompanyValidation();

    private static async Task<Ok<List<TicketHistoryDto>>> Handle(
        [FromRoute] string ticketId,
        ITicketHistoryService historyService
    )
    {
        var history = await historyService.GetTicketHistoryAsync(ticketId);
        return TypedResults.Ok(history);
    }
}
