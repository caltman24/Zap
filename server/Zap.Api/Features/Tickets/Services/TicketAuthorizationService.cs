using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Data;

namespace Zap.Api.Features.Tickets.Services;

public sealed class TicketAuthorizationService(AppDbContext db) : ITicketAuthorizationService
{
    public async Task<bool> CanReadTicketAsync(string ticketId, CurrentUser currentUser)
    {
        var context = await GetTicketAccessContextAsync(ticketId);
        if (context == null || !IsSameCompany(context, currentUser)) return false;

        return currentUser.Member!.Role.Name switch
        {
            RoleNames.Admin => true,
            RoleNames.ProjectManager => context.ProjectManagerId == currentUser.Member.Id,
            RoleNames.Developer => context.ProjectMemberIds.Contains(currentUser.Member.Id),
            RoleNames.Submitter =>
                context.ProjectMemberIds.Contains(currentUser.Member.Id) || context.SubmitterId == currentUser.Member.Id,
            _ => false
        };
    }

    public async Task<bool> CanCreateTicketInProjectAsync(string projectId, CurrentUser currentUser)
    {
        var projectContext = await db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => new
            {
                p.CompanyId,
                p.ProjectManagerId,
                p.IsArchived,
                AssignedMemberIds = p.AssignedMembers.Select(m => m.Id).ToList()
            })
            .FirstOrDefaultAsync();

        if (projectContext == null || currentUser.Member == null) return false;
        if (projectContext.CompanyId != currentUser.Member.CompanyId || projectContext.IsArchived) return false;

        return currentUser.Member.Role.Name switch
        {
            RoleNames.Admin => true,
            RoleNames.ProjectManager => projectContext.ProjectManagerId == currentUser.Member.Id,
            RoleNames.Submitter => projectContext.AssignedMemberIds.Contains(currentUser.Member.Id),
            _ => false
        };
    }

    public async Task<bool> CanEditTicketDetailsAsync(string ticketId, CurrentUser currentUser)
    {
        var context = await GetTicketAccessContextAsync(ticketId);
        if (context == null || !IsSameCompany(context, currentUser)) return false;

        return currentUser.Member!.Role.Name switch
        {
            RoleNames.Admin => true,
            RoleNames.ProjectManager => context.ProjectManagerId == currentUser.Member.Id,
            RoleNames.Submitter => context.SubmitterId == currentUser.Member.Id && context.Status == TicketStatuses.New,
            _ => false
        };
    }

    public async Task<bool> CanUpdateStatusAsync(string ticketId, CurrentUser currentUser)
    {
        var context = await GetTicketAccessContextAsync(ticketId);
        if (context == null || !IsSameCompany(context, currentUser)) return false;

        return currentUser.Member!.Role.Name switch
        {
            RoleNames.Admin => true,
            RoleNames.ProjectManager => context.ProjectManagerId == currentUser.Member.Id,
            RoleNames.Developer => context.AssigneeId == currentUser.Member.Id,
            _ => false
        };
    }

    public async Task<bool> CanUpdatePriorityAsync(string ticketId, CurrentUser currentUser)
    {
        var context = await GetTicketAccessContextAsync(ticketId);
        if (context == null || !IsSameCompany(context, currentUser)) return false;

        return currentUser.Member!.Role.Name switch
        {
            RoleNames.Admin => true,
            RoleNames.ProjectManager => context.ProjectManagerId == currentUser.Member.Id,
            _ => false
        };
    }

    public async Task<bool> CanUpdateTypeAsync(string ticketId, CurrentUser currentUser)
    {
        return await CanUpdatePriorityAsync(ticketId, currentUser);
    }

    public async Task<bool> CanAssignDeveloperAsync(string ticketId, CurrentUser currentUser)
    {
        return await CanUpdatePriorityAsync(ticketId, currentUser);
    }

    public async Task<bool> CanArchiveTicketAsync(string ticketId, CurrentUser currentUser)
    {
        return await CanUpdatePriorityAsync(ticketId, currentUser);
    }

    public async Task<bool> CanDeleteTicketAsync(string ticketId, CurrentUser currentUser)
    {
        return await CanUpdatePriorityAsync(ticketId, currentUser);
    }

    private async Task<TicketAccessContext?> GetTicketAccessContextAsync(string ticketId)
    {
        return await db.Tickets
            .Where(t => t.Id == ticketId)
            .Select(t => new TicketAccessContext(
                t.Project.CompanyId,
                t.Project.ProjectManagerId,
                t.SubmitterId,
                t.AssigneeId,
                t.Status.Name,
                t.Project.AssignedMembers.Select(m => m.Id).ToList()
            ))
            .FirstOrDefaultAsync();
    }

    private static bool IsSameCompany(TicketAccessContext context, CurrentUser currentUser)
    {
        return currentUser.Member != null && context.CompanyId == currentUser.Member.CompanyId;
    }

    private sealed record TicketAccessContext(
        string CompanyId,
        string? ProjectManagerId,
        string SubmitterId,
        string? AssigneeId,
        string Status,
        List<string> ProjectMemberIds
    );
}
