using Zap.Api.Data.Models;

namespace Zap.Api.Features.Tickets.Services;

// INFO: Strategy Pattern babyyy. We love to see it
public class CreatedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Ticket created by: {entry.Creator.User.FullName} on {entry.CreatedAt:g}";
    }
}

public class UpdateNameFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Name updated from '{entry.OldValue}' to '{entry.NewValue}' by {entry.Creator.User.FullName}";
    }
}

public class UpdateDescriptionFormatter : ITicketHistoryFormatter
{
    // not putting the description values inside the text due to possible long text
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return "Description updated";
    }
}

public class UpdateStatusFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Status updated from '{entry.OldValue}' to '{entry.NewValue}' by {entry.Creator.User.FullName}";
    }
}

public class UpdateTypeFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Type updated from '{entry.OldValue}' to '{entry.NewValue}' by {entry.Creator.User.FullName}";
    }
}

public class UpdatePriorityFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Priority updated from '{entry.OldValue}' to '{entry.NewValue}' by {entry.Creator.User.FullName}";
    }
}

public class ArchivedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Moved to Archived by {entry.Creator.User.FullName}";
    }
}

public class UnarchivedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Moved from Archived by {entry.Creator.User.FullName}";
    }
}

public class ResolvedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Marked as resolved by {entry.Creator.User.FullName}";
    }
}

public class DeveloperAssignedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Assigned to {entry.RelatedEntityName} by {entry.Creator.User.FullName}";
    }
}

public class DeveloperRemovedFormatter : ITicketHistoryFormatter
{
    public static string FormatHistoryEntry(TicketHistory entry)
    {
        return $"Assigned developer {entry.RelatedEntityName} removed by {entry.Creator.User.FullName}";
    }
}
