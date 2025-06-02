using Zap.Api.Data.Models;

namespace Zap.Api.Features.Tickets.Services;

// INFO: Strategy Pattern babyyy. We love to see it
public class CreatedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
        $"Ticket created by: {entry.Creator.User.FullName} on {entry.CreatedAt:g}";
}
public class UpdateNameFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Name updated from '{entry.OldValue}' to '{entry.NewValue}' by {entry.Creator.User.FullName}";
}
public class UpdateDescriptionFormatter : ITicketHistoryFormatter
{
    // not putting the description values inside the text due to possible long text
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Description updated";
}
public class UpdateStatusFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Status updated from '{entry.OldValue}' to '{entry.NewValue}' by {entry.Creator.User.FullName}";
}
public class UpdateTypeFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Type updated from '{entry.OldValue}' to '{entry.NewValue}' by {entry.Creator.User.FullName}";
}
public class UpdatePriorityFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Priority updated from '{entry.OldValue}' to '{entry.NewValue}' by {entry.Creator.User.FullName}";
}
public class ArchivedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Moved to Archived by {entry.Creator.User.FullName}";
}
public class UnarchivedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Moved from Archived by {entry.Creator.User.FullName}";
}
public class ResolvedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Marked as resolved by {entry.Creator.User.FullName}";
}
public class DeveloperAssignedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Assigned to {entry.RelatedEntityName} by {entry.Creator.User.FullName}";
}
public class DeveloperRemovedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry) =>
            $"Assigned developer {entry.Creator.User.FullName} removed by {entry.Creator.User.FullName}";
}

