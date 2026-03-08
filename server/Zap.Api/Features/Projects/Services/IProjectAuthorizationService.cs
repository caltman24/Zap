using Zap.Api.Common.Authorization;

namespace Zap.Api.Features.Projects.Services;

public interface IProjectAuthorizationService
{
    Task<bool> CanReadProjectAsync(string projectId, CurrentUser currentUser);
}
