using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;

namespace Zap.Api.Features.Tickets;

public class CreateTicket : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle);

    private static Ok Handle()
    {
        // This is where we create tickets
        // Here, we assign the ticket submitter, and assign a developer

        // Anyone assigned to the project can submit.
        // BUT ONLY PROJECT MANAGERS and ADMINS can assign a developer
        return TypedResults.Ok();
    }
}

