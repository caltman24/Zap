namespace Zap.Api.Features.Tickets.Services;

public interface ITicketService
{
    public Task<TicketDto> CreateTicketAsync(CreateTicketDto ticket);
    public Task<TicketDto?> GetTicketByIdAsync(string ticketId);
}

public record TicketDto(
    string Id,
    string Name,
    string Description,
    string Priority,
    string Status,
    string Type,
    string ProjectId,
    string SubmitterId,
    string? AssigneeId
);

public record CreateTicketDto(
    string Name,
    string Description,
    string Priority,
    string Status,
    string Type,
    string ProjectId,
    string SubmitterId,
    string? AssigneeId
);
