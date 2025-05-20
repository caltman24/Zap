using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Data;

namespace Zap.Api.Features.Projects.Filters;

internal static class ProjectFiltersExtensions
{
    ///<summary>
    /// Gets the first argument of projectId to validate relationship with current user
    ///</summary>
    /// <returns>
    /// Returns <see cref="TypedResults.NotFound"/> if the resource is not found,
    /// <see cref="TypedResults.Forbid"/> if the user is not authorized,
    /// or the result of <paramref name="next"/> if neither condition is met.
    /// </returns>
    internal static RouteHandlerBuilder WithProjectTicketValidation(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<ProjectCompanyValidationFilter>();

    private class ProjectCompanyValidationFilter(AppDbContext db, CurrentUser currentUser)
        : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var projectId = context.GetArgument<string>(0);

            var projectCompanyId = await db.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.CompanyId)
                .FirstOrDefaultAsync();

            if (projectCompanyId == null) return TypedResults.NotFound();

            if (projectCompanyId != currentUser.Member!.CompanyId)
            {
                return TypedResults.Forbid();
            }


            return await next(context);
        }
    }
}

