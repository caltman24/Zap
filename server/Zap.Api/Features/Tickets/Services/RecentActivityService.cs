using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Enums;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public class RecentActivityService(AppDbContext db) : IRecentActivityService
{
    public async Task<List<RecentActivityDto>> GetRecentActivityAsync(CurrentUser currentUser, int limit = 5)
    {
        if (currentUser.Member == null || string.IsNullOrWhiteSpace(currentUser.CompanyId) || limit <= 0) return [];

        var lifecycleEventsTask = GetLifecycleEventsAsync(currentUser, limit);
        var commentEventsTask = GetCommentEventsAsync(currentUser, limit);

        await Task.WhenAll(lifecycleEventsTask, commentEventsTask);

        return lifecycleEventsTask.Result
            .Concat(commentEventsTask.Result)
            .OrderByDescending(activity => activity.OccurredAt)
            .Take(limit)
            .ToList();
    }

    private async Task<List<RecentActivityDto>> GetLifecycleEventsAsync(CurrentUser currentUser, int limit)
    {
        var member = currentUser.Member!;
        var visibleTickets = GetLifecycleVisibleTicketsQuery(member.Id, member.Role.Name, currentUser.CompanyId!);
        var visibleTicketIds = visibleTickets.Select(ticket => ticket.Id);

        var historyEntries = await db.TicketHistories
            .AsNoTracking()
            .Where(history => visibleTicketIds.Contains(history.TicketId))
            .Where(history => history.Type == TicketHistoryTypes.Created
                              || history.Type == TicketHistoryTypes.UpdateStatus
                              || history.Type == TicketHistoryTypes.UpdatePriority
                              || history.Type == TicketHistoryTypes.Resolved
                              || history.Type == TicketHistoryTypes.DeveloperAssigned
                              || history.Type == TicketHistoryTypes.DeveloperRemoved)
            .OrderByDescending(history => history.CreatedAt)
            .Take(limit)
            .Select(history => new HistoryActivityProjection(
                history.Id,
                history.TicketId,
                history.Ticket.Name,
                history.Ticket.ProjectId,
                history.Type,
                new MemberInfoDto(
                    history.Creator.Id,
                    history.Creator.User.FullName,
                    history.Creator.User.AvatarUrl,
                    history.Creator.Role.Name),
                history.CreatedAt,
                history.OldValue,
                history.NewValue,
                history.RelatedEntityName))
            .ToListAsync();

        return historyEntries
            .Select(MapHistoryEntry)
            .ToList();
    }

    private async Task<List<RecentActivityDto>> GetCommentEventsAsync(CurrentUser currentUser, int limit)
    {
        var member = currentUser.Member!;

        if (member.Role.Name == RoleNames.Admin) return [];

        var visibleTickets = GetCommentVisibleTicketsQuery(member.Id, member.Role.Name, currentUser.CompanyId!);
        var visibleTicketIds = visibleTickets.Select(ticket => ticket.Id);

        var comments = await db.TicketComments
            .AsNoTracking()
            .Where(comment => visibleTicketIds.Contains(comment.TicketId))
            .OrderByDescending(comment => comment.CreatedAt)
            .Take(limit)
            .Select(comment => new CommentActivityProjection(
                comment.Id,
                comment.TicketId,
                comment.Ticket.Name,
                comment.Ticket.ProjectId,
                new MemberInfoDto(
                    comment.Sender.Id,
                    comment.Sender.User.FullName,
                    comment.Sender.User.AvatarUrl,
                    comment.Sender.Role.Name),
                comment.CreatedAt,
                comment.Message))
            .ToListAsync();

        return comments
            .Select(comment => new RecentActivityDto(
                comment.Id,
                comment.TicketId,
                comment.TicketName,
                comment.ProjectId,
                RecentActivityTypes.CommentAdded,
                comment.Actor,
                comment.OccurredAt,
                comment.Message,
                null,
                null,
                null))
            .ToList();
    }

    private IQueryable<Ticket> GetLifecycleVisibleTicketsQuery(string memberId, string roleName, string companyId)
    {
        var query = db.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.Project.CompanyId == companyId && !ticket.IsArchived && !ticket.Project.IsArchived);

        return roleName switch
        {
            RoleNames.Admin => query,
            RoleNames.ProjectManager => query.Where(ticket => ticket.Project.ProjectManagerId == memberId),
            RoleNames.Developer => query.Where(ticket =>
                ticket.Project.AssignedMembers.Any(member => member.Id == memberId)),
            RoleNames.Submitter => query.Where(ticket =>
                ticket.Project.AssignedMembers.Any(member => member.Id == memberId) || ticket.SubmitterId == memberId),
            _ => query.Where(_ => false)
        };
    }

    private IQueryable<Ticket> GetCommentVisibleTicketsQuery(string memberId, string roleName, string companyId)
    {
        var query = db.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.Project.CompanyId == companyId && !ticket.IsArchived && !ticket.Project.IsArchived);

        return roleName switch
        {
            RoleNames.ProjectManager => query.Where(ticket => ticket.Project.ProjectManagerId == memberId),
            RoleNames.Developer => query.Where(ticket => ticket.AssigneeId == memberId),
            RoleNames.Submitter => query.Where(ticket => ticket.SubmitterId == memberId),
            _ => query.Where(_ => false)
        };
    }

    private static RecentActivityDto MapHistoryEntry(HistoryActivityProjection historyEntry)
    {
        var activityType = historyEntry.Type switch
        {
            TicketHistoryTypes.Created => RecentActivityTypes.TicketCreated,
            TicketHistoryTypes.UpdateStatus => RecentActivityTypes.StatusChanged,
            TicketHistoryTypes.Resolved => RecentActivityTypes.StatusChanged,
            TicketHistoryTypes.UpdatePriority => RecentActivityTypes.PriorityChanged,
            TicketHistoryTypes.DeveloperAssigned => RecentActivityTypes.AssigneeChanged,
            TicketHistoryTypes.DeveloperRemoved => RecentActivityTypes.AssigneeChanged,
            _ => throw new InvalidOperationException($"Unsupported history type '{historyEntry.Type}'.")
        };

        var oldValue = historyEntry.Type == TicketHistoryTypes.DeveloperRemoved
            ? historyEntry.RelatedEntityName
            : historyEntry.OldValue;
        var newValue = historyEntry.Type switch
        {
            TicketHistoryTypes.DeveloperAssigned => historyEntry.RelatedEntityName,
            TicketHistoryTypes.Resolved => TicketStatuses.Resolved,
            _ => historyEntry.NewValue
        };

        return new RecentActivityDto(
            historyEntry.Id,
            historyEntry.TicketId,
            historyEntry.TicketName,
            historyEntry.ProjectId,
            activityType,
            historyEntry.Actor,
            historyEntry.OccurredAt,
            null,
            oldValue,
            newValue,
            historyEntry.RelatedEntityName);
    }

    private sealed record HistoryActivityProjection(
        string Id,
        string TicketId,
        string TicketName,
        string ProjectId,
        TicketHistoryTypes Type,
        MemberInfoDto Actor,
        DateTime OccurredAt,
        string? OldValue,
        string? NewValue,
        string? RelatedEntityName);

    private sealed record CommentActivityProjection(
        string Id,
        string TicketId,
        string TicketName,
        string ProjectId,
        MemberInfoDto Actor,
        DateTime OccurredAt,
        string Message);
}