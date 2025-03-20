using Microsoft.AspNetCore.Identity;

namespace Zap.DataAccess.Models;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public string? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Company? OwnedCompany { get; set; }

    public ICollection<Project> AssignedProjects { get; set; } = [];
}