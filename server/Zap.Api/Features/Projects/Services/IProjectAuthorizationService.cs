using Zap.Api.Common.Authorization;

namespace Zap.Api.Features.Projects.Services;

public interface IProjectAuthorizationService
{
    Task<bool> CanReadProjectAsync(string projectId, CurrentUser currentUser);

    ProjectCapabilitiesDto GetCapabilities(ProjectDto project, CurrentUser currentUser);
}

public record ProjectCapabilitiesDto(
    bool CanEdit,
    bool CanArchive,
    bool CanAssignProjectManager,
    bool CanManageMembers,
    bool CanCreateTicket);
