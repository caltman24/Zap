﻿using System.ComponentModel.DataAnnotations;

namespace Zap.Api.Data.Models;

public class Ticket
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(50)] public required string Name { get; set; }
    [StringLength(1000)] public required string Description { get; set; }
    public bool IsArchived { get; set; } = false;

    public required string ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public required string PriorityId { get; set; }
    public TicketPriority Priority { get; set; } = null!;

    public required string StatusId { get; set; }
    public TicketStatus Status { get; set; } = null!;

    public required string TypeId { get; set; }
    public TicketType Type { get; set; } = null!;

    public required string SubmitterId { get; set; }
    public CompanyMember Submitter { get; set; } = null!;

    public string? AssigneeId { get; set; }
    public CompanyMember? Assignee { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = [];
    public ICollection<TicketAttachment> Attachments { get; set; } = [];
    public ICollection<TicketHistory> History { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = null;
}
