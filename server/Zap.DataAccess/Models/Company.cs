namespace Zap.DataAccess.Models;

public class Company
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    
    public string? OwnerId { get; set; }
    public virtual AppUser? Owner { get; set; }

    public virtual ICollection<AppUser> Members { get; set; } = [];
    public virtual ICollection<Project> Projects { get; set; } = [];
}