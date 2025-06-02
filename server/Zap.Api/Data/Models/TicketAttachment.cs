using System.ComponentModel.DataAnnotations;

namespace Zap.Api.Data.Models;

public class TicketAttachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public required string TicketId { get; set; }
    public Ticket Ticket { get; set; } = default!;

    public required string OwnerId { get; set; }
    public CompanyMember Owner { get; set; } = default!;

    public string StoreKey { get; set; } = default!;
    public string StoreUrl { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = default!;


    // Name of file
    // size of file
    // type of file
    // s3: object key / url. (Url embeds object key inside)

}
