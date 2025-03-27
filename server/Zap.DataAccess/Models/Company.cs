using System.ComponentModel.DataAnnotations;

namespace Zap.DataAccess.Models;

public class Company
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(75)] public required string Name { get; set; }

    [StringLength(1000)] public required string Description { get; set; }

    [StringLength(500)] public string? LogoUrl { get; set; }
    [StringLength(500)]
    public string? LogoKey { get; set; }

    [StringLength(500)] public string? WebsiteUrl { get; set; }

    public string? OwnerId { get; set; }
    public virtual AppUser? Owner { get; set; }

    public virtual ICollection<AppUser> Members { get; set; } = [];
    public virtual ICollection<Project> Projects { get; set; } = [];
}