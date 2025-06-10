using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Enums;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public class TicketHistoryService(AppDbContext db) : ITicketHistoryService
{
    public async Task CreateHistoryEntryAsync(string ticketId, string creatorId, TicketHistoryTypes type, string? oldValue = null, string? newValue = null, string? relatedEntityName = null, string? relatedEntityId = null)
    {
        var historyEntry = new TicketHistory
        {
            TicketId = ticketId,
            CreatorId = creatorId,
            Type = type,
            OldValue = oldValue,
            NewValue = newValue,
            RelatedEntityName = relatedEntityName,
            RelatedEntityId = relatedEntityId,
            CreatedAt = DateTime.UtcNow
        };

        await db.TicketHistories.AddAsync(historyEntry);
        await db.SaveChangesAsync();
    }

    public async Task<List<TicketHistoryDto>> GetTicketHistoryAsync(string ticketId)
    {
        var historyEntries = await db.TicketHistories
            .Where(h => h.TicketId == ticketId)
            .Include(h => h.Creator)
            .ThenInclude(c => c.User)
            .Include(h => h.Creator)
            .ThenInclude(c => c.Role)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();

        return historyEntries.Select(h => new TicketHistoryDto(
            h.Id,
            h.Type,
            h.OldValue,
            h.NewValue,
            h.RelatedEntityName,
            h.RelatedEntityId,
            new MemberInfoDto(
                h.Creator.Id,
                $"{h.Creator.User.FirstName} {h.Creator.User.LastName}",
                h.Creator.User.AvatarUrl,
                h.Creator.Role.Name
            ),
            h.CreatedAt,
            FormatHistoryEntry(h)
        )).ToList();
    }

    private static string FormatHistoryEntry(TicketHistory entry)
    {
        return entry.Type switch
        {
            TicketHistoryTypes.Created => CreatedFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.UpdateName => UpdateNameFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.UpdateDescription => UpdateDescriptionFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.UpdateStatus => UpdateStatusFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.UpdateType => UpdateTypeFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.UpdatePriority => UpdatePriorityFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.Archived => ArchivedFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.Unarchived => UnarchivedFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.Resolved => ResolvedFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.DeveloperAssigned => DeveloperAssignedFormatter.FormatHistoryEntry(entry),
            TicketHistoryTypes.DeveloperRemoved => DeveloperRemovedFormatter.FormatHistoryEntry(entry),
            _ => "Unknown action"
        };
    }
}