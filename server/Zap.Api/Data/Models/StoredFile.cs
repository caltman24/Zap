namespace Zap.Api.Data.Models;

// A stored file includes ticket attachments but also public images/private file uploads
public class StoredFile : BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public required string CompanyId { get; set; }
    public Company Company { get; set; } = default!;

    public string Category { get; set; } = default!; // "TicketAttachment", "Avatar", "CompanyLogo"
    public string Visibility { get; set; } = default!; // "Private", "Public"

    public string BucketName { get; set; } = default!;
    public string ObjectKey { get; set; } = default!;

    public string OriginalFileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }

    public required string OwnerId { get; set; }
    public CompanyMember Owner { get; set; } = default!;

    public TicketAttachment? TicketAttachment { get; set; }
}