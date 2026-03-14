namespace Zap.Api.Data.Models;

// will always be a file uploaded to s3 storage
public class TicketAttachment : BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public required string TicketId { get; set; }
    public Ticket Ticket { get; set; } = default!;

    public required string OwnerId { get; set; }
    public CompanyMember Owner { get; set; } = default!;

    public required string FileId { get; set; }
    public StoredFile File { get; set; } = default!;
}
