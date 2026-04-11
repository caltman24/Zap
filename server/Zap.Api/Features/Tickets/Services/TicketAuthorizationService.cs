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
            RoleNames.ProjectManager => true,
            RoleNames.Developer => context.ProjectMemberIds.Contains(currentUser.Member.Id),
            RoleNames.Submitter =>
                context.ProjectMemberIds.Contains(currentUser.Member.Id) ||
                context.SubmitterId == currentUser.Member.Id,
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

        var isSubmitterWithEditableStatus = context.SubmitterId == currentUser.Member!.Id &&
                                            context.Status == TicketStatuses.New;

        return currentUser.Member!.Role.Name switch
        {
            RoleNames.Admin => true,
            RoleNames.ProjectManager => context.ProjectManagerId == currentUser.Member.Id ||
                                        isSubmitterWithEditableStatus,
            RoleNames.Submitter => isSubmitterWithEditableStatus,
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


    // HACK: These are coupled to the above permissions checks since they follow same permissions. 
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

    public async Task<bool> CanCommentTicketAsync(string ticketId, CurrentUser currentUser)
    {
        var context = await GetTicketAccessContextAsync(ticketId);
        if (context == null || !IsSameCompany(context, currentUser)) return false;

        return TicketAuthorizationRules.CanCommentOnTicket(
            context.ProjectManagerId,
            context.SubmitterId,
            context.AssigneeId,
            currentUser);
    }

    public TicketCapabilitiesDto GetCapabilities(BasicTicketDto ticket, CurrentUser currentUser)
    {
        if (currentUser.Member == null)
            return new TicketCapabilitiesDto(false, false, false, false, false, false, false, false, false, false);

        var member = currentUser.Member;
        var isAdmin = member.Role.Name == RoleNames.Admin;
        var isProjectManager = ticket.ProjectManagerId == member.Id;
        var isSubmitter = ticket.Submitter.Id == member.Id;
        var isAssignedDeveloper = ticket.Assignee?.Id == member.Id;
        var canManageTicket = isAdmin || isProjectManager;
        var canComment = TicketAuthorizationRules.CanCommentOnTicket(
            ticket.ProjectManagerId,
            ticket.Submitter.Id,
            ticket.Assignee?.Id,
            currentUser);
        var canEditNameDescription = isAdmin || isProjectManager ||
                                     (!ticket.isArchived && isSubmitter && ticket.Status == TicketStatuses.New);
        var canUpdatePriority = !ticket.isArchived && canManageTicket;
        var canUpdateStatus = !ticket.isArchived &&
                              (canManageTicket || (member.Role.Name == RoleNames.Developer && isAssignedDeveloper));

        return new TicketCapabilitiesDto(
            canEditNameDescription || canUpdatePriority || canUpdateStatus,
            canEditNameDescription,
            canUpdatePriority,
            canUpdateStatus,
            canUpdatePriority,
            !ticket.isArchived && canManageTicket,
            canManageTicket,
            canManageTicket,
            !ticket.isArchived && canManageTicket,
            canComment);
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
                t.IsArchived,
                t.Project.AssignedMembers.Select(m => m.Id).ToList()
            ))
            .FirstOrDefaultAsync();
    }

    private static bool IsTicketManager(TicketAccessContext context, CurrentUser currentUser)
    {
        if (currentUser.Member == null) return false;

        return currentUser.Member.Role.Name == RoleNames.Admin ||
               context.ProjectManagerId == currentUser.Member.Id;
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
        bool IsArchived,
        List<string> ProjectMemberIds
    );
}