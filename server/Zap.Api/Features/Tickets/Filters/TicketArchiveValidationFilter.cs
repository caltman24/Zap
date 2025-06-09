using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;

namespace Zap.Api.Features.Tickets.Filters;

internal static class TicketArchiveFiltersExtensions
{
    /// <summary>
    /// Validates that the ticket is not archived before allowing operations.
    /// Only allows archive/unarchive and name/description updates on archived tickets.
    /// </summary>
    /// <returns>
    /// Returns <see cref="TypedResults.NotFound"/> if the resource is not found,
    /// <see cref="TypedResults.BadRequest"/> if the ticket is archived and operation is not allowed,
    /// or the result of <paramref name="next"/> if neither condition is met.
    /// </returns>
    internal static RouteHandlerBuilder WithTicketArchiveValidation(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<TicketArchiveValidationFilter>();

    private class TicketArchiveValidationFilter(AppDbContext db) : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var ticketId = context.GetArgument<string>(0);

            var ticket = await db.Tickets
                .Where(t => t.Id == ticketId)
                .Select(t => new { t.IsArchived })
                .FirstOrDefaultAsync();

            if (ticket == null) return TypedResults.NotFound();

            // If ticket is archived, only allow specific operations
            if (ticket.IsArchived)
            {
                var httpContext = context.HttpContext;
                var endpoint = httpContext.GetEndpoint();
                var endpointName = endpoint?.DisplayName ?? "";

                // Allow archive/unarchive operations
                if (endpointName.Contains("ArchiveTicket"))
                {
                    return await next(context);
                }

                // Allow name/description updates only (check if it's UpdateTicket with limited fields)
                if (endpointName.Contains("UpdateTicket"))
                {
                    // This will be handled by a modified UpdateTicket endpoint that checks for archived status
                    return await next(context);
                }

                // Block all other operations on archived tickets
                return TypedResults.BadRequest("Cannot perform this operation on an archived ticket. Only unarchiving and editing name/description are allowed.");
            }

            return await next(context);
        }
    }
}
