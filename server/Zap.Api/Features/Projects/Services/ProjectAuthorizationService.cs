using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Data;

namespace Zap.Api.Features.Projects.Services;

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
            RoleNames.ProjectManager => context.ProjectManagerId == currentUser.Member.Id,
            RoleNames.Developer => context.AssignedMemberIds.Contains(currentUser.Member.Id),
            RoleNames.Submitter => context.AssignedMemberIds.Contains(currentUser.Member.Id),
            _ => false
        };
    }

    private sealed record ProjectAccessContext(
        string CompanyId,
        string? ProjectManagerId,
        List<string> AssignedMemberIds);
}
