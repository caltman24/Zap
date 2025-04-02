using System.ComponentModel.DataAnnotations;

namespace Zap.DataAccess.Models;

public class Project
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [StringLength(50)] public required string Name { get; set; }
    [StringLength(1000)] public required string Description { get; set; }
    [StringLength(50)] public required string Priority { get; set; }
    
    public DateTime DueDate { get; set; }
    public bool IsArchived { get; set; }
    
    public required string CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;
    
    public virtual ICollection<AppUser> AssignedMembers { get; set; } = [];
    public virtual ICollection<Ticket> Tickets { get; set; } = [];
}
