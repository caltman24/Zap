using System.ComponentModel.DataAnnotations;

namespace Zap.Api.Data.Models;

public class Ticket
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [StringLength(50)] public required string Name { get; set; }
    [StringLength(1000)] public required string Description { get; set; }
    [StringLength(50)] public required string Priority { get; set; }
    [StringLength(50)] public required string Status { get; set; }
    [StringLength(50)] public required string Type { get; set; }

    public required string SubmitterId { get; set; }
    public AppUser Submitter { get; set; } = null!;

    public string? AssigneeId { get; set; }
    public AppUser? Assignee { get; set; }
}