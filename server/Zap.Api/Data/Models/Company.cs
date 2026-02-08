using System.ComponentModel.DataAnnotations;

namespace Zap.Api.Data.Models;

public class Company : BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(75)] public required string Name { get; set; }

    [StringLength(1000)] public required string Description { get; set; }

    [StringLength(500)] public string? LogoUrl { get; set; }

    [StringLength(500)] public string? LogoKey { get; set; }

    [StringLength(500)] public string? WebsiteUrl { get; set; }

    public string? OwnerId { get; set; }
    public AppUser? Owner { get; set; }

    public ICollection<CompanyMember> Members { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
}