namespace Zap.DataAccess.Models;

public class Project
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public DateTime DueDate { get; set; }
    
    public string? CompanyId { get; set; }
    public Company? Company { get; set; }
    
    public ICollection<AppUser> AssignedMembers { get; set; } = [];
    public ICollection<Ticket> Tickets { get; set; } = [];
}