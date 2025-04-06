using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Zap.DataAccess.Models;

namespace Zap.Api.Common.Extensions;

public static class UserManagerExtension
{
    public static async Task<IdentityResult> AddCustomClaimsAsync(this UserManager<AppUser> userManager, AppUser user)
    {
        return await userManager.AddClaimsAsync(user, [
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
        ]);
    }
    
}