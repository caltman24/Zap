using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Data;
using Zap.Api.Features.Projects.Services;

namespace Zap.Api.Features.Projects.Filters;

internal static class ProjectFiltersExtensions
{
    /// <summary>
    ///     Gets the first argument of projectId to validate relationship with current user
    /// </summary>
    /// <returns>
    ///     Returns <see cref="TypedResults.NotFound" /> if the resource is not found,
    ///     <see cref="TypedResults.Forbid" /> if the user is not authorized,
    ///     or the result of <paramref name="next" /> if neither condition is met.
    /// </returns>
    internal static RouteHandlerBuilder WithProjectAccessValidation(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<ProjectAccessValidationFilter>();
    }

    private class ProjectAccessValidationFilter(
        AppDbContext db,
        CurrentUser currentUser,
        IProjectAuthorizationService projectAuthorizationService)
        : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var projectId = context.GetArgument<string>(0);

            var exists = await db.Projects.AnyAsync(p => p.Id == projectId);
            if (!exists) return TypedResults.NotFound();

            if (!await projectAuthorizationService.CanReadProjectAsync(projectId, currentUser))
                return TypedResults.Forbid();

            return await next(context);
        }
    }
}