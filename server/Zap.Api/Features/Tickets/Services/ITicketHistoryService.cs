using Zap.Api.Common;
using Zap.Api.Common.Enums;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketHistoryService
{
    Task CreateHistoryEntryAsync(string ticketId, string creatorId, TicketHistoryTypes type, string? oldValue = null, string? newValue = null, string? relatedEntityName = null, string? relatedEntityId = null);
    Task<List<TicketHistoryDto>> GetTicketHistoryAsync(string ticketId);
    Task<PaginatedResponse<TicketHistoryDto>> GetTicketHistoryAsync(string ticketId, int page, int pageSize);
}

public record TicketHistoryDto(
    string Id,
    TicketHistoryTypes Type,
    string? OldValue,
    string? NewValue,
    string? RelatedEntityName,
    string? RelatedEntityId,
    MemberInfoDto Creator,
    DateTime CreatedAt,
    string FormattedMessage
);
