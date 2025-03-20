namespace Zap.DataAccess.Models;

public class Company
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    
    public string? OwnerId { get; set; }
    public AppUser? Owner { get; set; }

    public ICollection<AppUser> Members { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
}