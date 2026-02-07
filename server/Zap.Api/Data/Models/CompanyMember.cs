namespace Zap.Api.Data.Models;

public class CompanyMember : BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = default!;
    public AppUser User { get; set; } = default!;

    public string? CompanyId { get; set; }
    public Company? Company { get; set; }

    public ICollection<Project> AssignedProjects { get; set; } = [];

    public string RoleId { get; set; } = default!;
    public CompanyRole Role { get; set; } = default!;
}