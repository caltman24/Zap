using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;

namespace Zap.Api.Features.Projects.Filters;

internal static class ActiveProjectFilterExtensions
{
    /// <summary>
    ///     Validates that the project is not archived before allowing operations.
    /// </summary>
    /// <returns>
    ///     Returns <see cref="TypedResults.NotFound" /> if the resource is not found,
    ///     <see cref="TypedResults.BadRequest" /> if the project is archived and operation is not allowed,
    ///     or the result of <paramref name="next" /> if neither condition is met.
    /// </returns>
    internal static RouteHandlerBuilder WithActiveProjectValidation(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<ActiveProjectValidationFilter>();
    }

    private class ActiveProjectValidationFilter(AppDbContext db) : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var projectId = context.GetArgument<string>(0);

            var project = await db.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new { p.IsArchived })
                .FirstOrDefaultAsync();

            if (project == null) return TypedResults.NotFound();

            // If project is archived, only allow specific operations
            if (project.IsArchived)
                return TypedResults.BadRequest(
                    "Cannot perform this operation on an archived project.");

            return await next(context);
        }
    }
}