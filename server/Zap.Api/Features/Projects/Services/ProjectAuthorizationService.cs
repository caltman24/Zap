using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Data;

namespace Zap.Api.Features.Projects.Services;

/// <summary>
///     Evaluates project read access for company members using role-based rules and project
///     assignment relationships.
/// </summary>
public sealed class ProjectAuthorizationService(AppDbContext db) : IProjectAuthorizationService
{
    public async Task<bool> CanReadProjectAsync(string projectId, CurrentUser currentUser)
    {
        if (currentUser.Member == null) return false;

        var context = await db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => new ProjectAccessContext(
                p.CompanyId,
                p.ProjectManagerId,
                p.AssignedMembers.Select(m => m.Id).ToList()))
            .FirstOrDefaultAsync();

        if (context == null || context.CompanyId != currentUser.Member.CompanyId) return false;

        return currentUser.Member.Role.Name switch
        {
            RoleNames.Admin => true,
            RoleNames.ProjectManager => true,
            RoleNames.Developer => context.AssignedMemberIds.Contains(currentUser.Member.Id),
            RoleNames.Submitter => context.AssignedMemberIds.Contains(currentUser.Member.Id),
            _ => false
        };
    }

    public ProjectCapabilitiesDto GetCapabilities(ProjectDto project, CurrentUser currentUser)
    {
        if (currentUser.Member == null)
        {
            return new ProjectCapabilitiesDto(false, false, false, false, false);
        }

        var isAdmin = currentUser.Member.Role.Name == RoleNames.Admin;
        var isProjectManager = project.ProjectManager?.Id == currentUser.Member.Id;
        var isAssignedMember = project.Members.Any(m => m.Id == currentUser.Member.Id);

        var canManageProject = isAdmin || isProjectManager;

        return new ProjectCapabilitiesDto(
            CanEdit: canManageProject,
            CanArchive: canManageProject,
            CanAssignProjectManager: isAdmin && !project.IsArchived,
            CanManageMembers: canManageProject && !project.IsArchived,
            CanCreateTicket: !project.IsArchived && (isAdmin || isProjectManager ||
                currentUser.Member.Role.Name == RoleNames.Submitter && isAssignedMember));
    }

    private sealed record ProjectAccessContext(
        string CompanyId,
        string? ProjectManagerId,
        List<string> AssignedMemberIds);
}
