using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Data;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets.Filters;

internal static class TicketFiltersExtensions
{
    /// <summary>
    ///     Gets the first argument of ticketId to validate relationship with current user
    /// </summary>
    /// <returns>
    ///     Returns <see cref="TypedResults.NotFound" /> if the resource is not found,
    ///     <see cref="TypedResults.Forbid" /> if the user is not authorized,
    ///     or the result of <paramref name="next" /> if neither condition is met.
    /// </returns>
    internal static RouteHandlerBuilder WithTicketAccessValidation(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<TicketAccessValidationFilter>();
    }

    private class TicketAccessValidationFilter(
        AppDbContext db,
        CurrentUser currentUser,
        ITicketAuthorizationService ticketAuthorizationService)
        : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var ticketId = context.GetArgument<string>(0);

            var exists = await db.Tickets.AnyAsync(t => t.Id == ticketId);
            if (!exists) return TypedResults.NotFound();

            if (!await ticketAuthorizationService.CanReadTicketAsync(ticketId, currentUser))
                return TypedResults.Forbid();

            return await next(context);
        }
    }
}
