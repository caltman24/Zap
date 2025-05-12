
using System.ComponentModel.DataAnnotations;

namespace Zap.Api.Data.Models;

public class TicketType
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [StringLength(50)] public required string Name { get; set; }
}
