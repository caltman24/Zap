using System.ComponentModel.DataAnnotations;
using Zap.Api.Common.Constants;

namespace Zap.Api.Data.Models;

public class CompanyMember
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = default!;
    public AppUser User { get; set; } = default!;

    public string? CompanyId { get; set; }
    public Company? Company { get; set; }


    public string RoleId { get; set; } = default!;
    public CompanyRole Role { get; set; } = default!;
}
