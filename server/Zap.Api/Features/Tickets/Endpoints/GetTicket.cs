
using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;

namespace Zap.Api.Features.Tickets;

public class GetTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{ticketId}", Handle);

    private static Ok Handle()
    {
        return TypedResults.Ok();
    }
}
