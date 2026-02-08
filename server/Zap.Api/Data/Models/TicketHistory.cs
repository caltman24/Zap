using Zap.Api.Common.Enums;

namespace Zap.Api.Data.Models;

public class TicketHistory : BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public required string TicketId { get; set; }
    public Ticket Ticket { get; set; } = default!;

    public required string CreatorId { get; set; }
    public CompanyMember Creator { get; set; } = default!;

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? RelatedEntityName { get; set; }
    public string? RelatedEntityId { get; set; }

    public TicketHistoryTypes Type { get; set; } = default!;
}