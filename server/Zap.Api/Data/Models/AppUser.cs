using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Zap.Api.Data.Models;

public class AppUser : IdentityUser
{
    [StringLength(50)] public required string FirstName { get; set; }
    [StringLength(50)] public required string LastName { get; set; }
    [StringLength(500)] public string AvatarUrl { get; set; } = "https://gravatar.com/avatar/HASH?d=mp";
    [StringLength(500)] public string? AvatarKey { get; set; } = null!;

    public void SetDefaultAvatar()
    {
        AvatarUrl = $"https://ui-avatars.com/api/?name={FirstName}+{LastName}";
    }
}
