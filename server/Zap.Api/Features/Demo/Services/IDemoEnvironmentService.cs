using Zap.Api.Data.Models;

namespace Zap.Api.Features.Demo.Services;

public interface IDemoEnvironmentService
{
    Task EnsureDemoEnvironmentAsync();
    Task<AppUser?> GetDemoUserByRoleAsync(string roleKey);
    Task ResetDemoEnvironmentAsync();
}
