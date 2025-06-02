using Zap.Api.Data.Models;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketHistoryFormatter
{
    abstract static string FormatHistoryEntry(TicketHistory entry);
}
