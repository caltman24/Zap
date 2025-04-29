using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;
using Zap.Api.Data.Models;

namespace Zap.Api.Common.Authorization;

internal static class CurrentUserExtensions
{
    internal static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddScoped<CurrentUser>();
        services.AddScoped<IClaimsTransformation, ClaimsTransformation>();

        return services;
    }

    private sealed class ClaimsTransformation(CurrentUser currentUser, UserManager<AppUser> userManager, AppDbContext db)
        : IClaimsTransformation
    {
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // We're not going to transform anything. We're using this as a hook into authorization
            // to set the current user without adding custom middleware.
            currentUser.Principal = principal;

            if (principal.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } id)
            {
                // Resolve the user manager and see if the current user is a valid user in the database
                // we do this once and store it on the current user.
                currentUser.User = await userManager.FindByIdAsync(id);
                if (currentUser.User != null)
                {
                    currentUser.Member = await db.CompanyMembers
                        .Where(m => m.UserId == currentUser.User.Id)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }
            }

            return principal;
        }
    }
}
