using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketService
{
    public Task<BasicTicketDto> CreateTicketAsync(CreateTicketDto ticket);
    public Task<BasicTicketDto?> GetTicketByIdAsync(string ticketId);
}

public record BasicTicketDto(
    string Id,
    string Name,
    string Description,
    string Priority,
    string Status,
    string Type,
    string ProjectId,
    MemberInfoDto Submitter,
    MemberInfoDto? Assignee
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
