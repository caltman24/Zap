using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;

namespace Zap.Api.Features.Projects.Filters;

internal static class ProjectArchiveFiltersExtensions
{
    /// <summary>
    ///     Validates that the project is not archived before allowing operations.
    ///     Only allows archive/unarchive and name/description updates on archived projects.
    /// </summary>
    /// <returns>
    ///     Returns <see cref="TypedResults.NotFound" /> if the resource is not found,
    ///     <see cref="TypedResults.BadRequest" /> if the project is archived and operation is not allowed,
    ///     or the result of <paramref name="next" /> if neither condition is met.
    /// </returns>
    internal static RouteHandlerBuilder WithProjectArchiveValidation(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<ProjectArchiveValidationFilter>();
    }

    private class ProjectArchiveValidationFilter(AppDbContext db) : IEndpointFilter
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
            {
                var httpContext = context.HttpContext;
                var endpoint = httpContext.GetEndpoint();
                var endpointName = endpoint?.DisplayName ?? "";

                // Allow archive/unarchive operations
                if (endpointName.Contains("ArchiveProject")) return await next(context);

                // Allow name/description updates only (check if it's UpdateProject with limited fields)
                if (endpointName.Contains("UpdateProject"))
                    // This will be handled by a modified UpdateProject endpoint that checks for archived status
                    return await next(context);

                // Block all other operations on archived projects
                return TypedResults.BadRequest(
                    "Cannot perform this operation on an archived project. Only unarchiving and editing name/description are allowed.");
            }

            return await next(context);
        }
    }
}