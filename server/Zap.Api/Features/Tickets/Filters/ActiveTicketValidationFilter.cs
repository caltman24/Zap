using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;

namespace Zap.Api.Features.Tickets.Filters;

internal static class ActiveTicketFiltersExtensions
{
    /// <summary>
    ///     Applies validation for endpoints that only support operations on active tickets.
    /// </summary>
    /// Returns
    /// <see cref="TypedResults.NotFound" />
    /// if the resource is not found,
    /// <see cref="TypedResults.BadRequest" />
    /// if the ticket is archived and operation is not allowed,
    /// or the result of
    /// <paramref name="next" />
    /// if neither condition is met.
    /// </returns>
    internal static RouteHandlerBuilder WithActiveTicketValidation(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<ActiveTicketValidationFilter>();
    }

    private class ActiveTicketValidationFilter(AppDbContext db) : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var ticketId = context.GetArgument<string>(0);

            var ticket = await db.Tickets
                .Where(t => t.Id == ticketId)
                .Select(t => new { t.IsArchived })
                .FirstOrDefaultAsync();

            if (ticket == null) return TypedResults.NotFound();

            if (ticket.IsArchived)
                return TypedResults.BadRequest(
                    "Cannot perform this operation on an archived ticket.");

            return await next(context);
        }
    }
}