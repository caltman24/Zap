using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Zap.DataAccess.Models;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string? CompanyId { get; set; }
    public string AvatarUrl { get; set; } = "https://gravatar.com/avatar/HASH?d=mp";
    public virtual Company? Company { get; set; }

    public virtual ICollection<Project> AssignedProjects { get; set; } = [];

    public void SetDefaultAvatar()
    {
        AvatarUrl = $"https://ui-avatars.com/api/?name={FirstName}+{LastName}";
    }
}