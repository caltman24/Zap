using Zap.Api.Data.Models;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketHistoryFormatter
{
    static abstract string FormatHistoryEntry(TicketHistory entry);
}