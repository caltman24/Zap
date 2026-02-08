using System.ComponentModel.DataAnnotations;

namespace Zap.Api.Data.Models;

public class TicketComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public required string TicketId { get; set; }
    public Ticket Ticket { get; set; } = default!;

    public required string SenderId { get; set; }
    public CompanyMember Sender { get; set; } = default!;

    [MaxLength(150)] public string Message { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}