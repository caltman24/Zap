using System.Security.Claims;
using Zap.DataAccess.Models;

namespace Zap.Api.Authorization;

public class CurrentUser
{
    public AppUser? User { get; set; }
    public ClaimsPrincipal Principal { get; set; } = null!;
    public string Id => Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public string Email => Principal.FindFirstValue(ClaimTypes.Email)!;
    public string Role => Principal.FindFirstValue(ClaimTypes.Role)!;
    public string? CompanyId => User?.CompanyId;
}