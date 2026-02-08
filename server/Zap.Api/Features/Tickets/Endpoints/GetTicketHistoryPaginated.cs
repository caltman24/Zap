using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Filters;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetTicketHistoryPaginated : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{ticketId}/history-pag", Handle)
            .WithName("GetTicketHistoryPaginated")
            .WithCompanyMember()
            .WithTicketCompanyValidation();
    }

    private static async Task<Ok<PaginatedResponse<TicketHistoryDto>>> Handle(
        [FromRoute] string ticketId,
        ITicketHistoryService historyService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        // Get paginated ticket history. The client will use this to display the history in a virtualized list.
        var historyPaginated = await historyService.GetTicketHistoryAsync(ticketId, page, pageSize);

        return TypedResults.Ok(historyPaginated);
    }
}