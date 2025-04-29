using System.ComponentModel.DataAnnotations;

namespace Zap.Api.Data.Models;

public class CompanyMember
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; }
    public AppUser User { get; set; }

    public string? CompanyId { get; set; }
    public Company? Company { get; set; }
}
