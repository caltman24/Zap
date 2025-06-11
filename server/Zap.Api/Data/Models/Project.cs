using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zap.Api.Data.Models;

public class Project : BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(50)] public required string Name { get; set; }
    [StringLength(1000)] public required string Description { get; set; }
    [StringLength(50)] public required string Priority { get; set; }
    public bool IsArchived { get; set; } = false;

    public required DateTime DueDate { get; set; }

    public required string CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string? ProjectManagerId { get; set; }
    public CompanyMember? ProjectManager { get; set; }

    public ICollection<CompanyMember> AssignedMembers { get; set; } = [];
    public ICollection<Ticket> Tickets { get; set; } = [];
}
