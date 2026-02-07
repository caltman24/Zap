using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
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
    internal static RouteHandlerBuilder WithTicketCompanyValidation(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<TicketCompanyValidationFilter>();
    }

    private class TicketCompanyValidationFilter(AppDbContext db, CurrentUser currentUser, ITicketService ticketService)
        : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var ticketId = context.GetArgument<string>(0);

            var ticketCompanyId = await db.Tickets
                .Where(t => t.Id == ticketId)
                .Select(t => t.Project.CompanyId)
                .FirstOrDefaultAsync();

            if (ticketCompanyId == null) return TypedResults.NotFound();


            if (ticketCompanyId != currentUser.Member!.CompanyId) return TypedResults.Forbid();


            var validMember = currentUser.Member!.Role.Name switch
            {
                RoleNames.Admin => true,
                RoleNames.ProjectManager =>
                    await ticketService.ValidateProjectManagerAsync(ticketId, currentUser.Member!.Id),
                _ =>
                    await ticketService.ValidateAssignedMemberAsync(ticketId, currentUser.Member!.Id)
            };

            if (!validMember) return TypedResults.Forbid();

            return await next(context);
        }
    }
}