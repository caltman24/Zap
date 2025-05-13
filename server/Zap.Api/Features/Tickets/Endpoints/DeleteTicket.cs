using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class DeleteTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{ticketId}", Handle)
            .WithName("DeleteTicket")
            .WithCompanyMember(RoleNames.Admin, RoleNames.ProjectManager);

    private static async Task<NoContent> Handle(
            [FromRoute] string ticketId,
            ITicketService ticketService
            )
    {
        await ticketService.DeleteTicketAsync(ticketId);

        return TypedResults.NoContent();
    }
}
