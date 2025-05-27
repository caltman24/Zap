using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketService
{
    public Task<CreateTicketResult> CreateTicketAsync(CreateTicketDto ticket);
    public Task<BasicTicketDto?> GetTicketByIdAsync(string ticketId);
    public Task<List<BasicTicketDto>> GetAssignedTicketsAsync(string memberId);
    public Task<List<BasicTicketDto>> GetOpenTicketsAsync(string companyId);
    public Task DeleteTicketAsync(string ticketId);

    ///<summary>
    /// Validate if the member is the actual project manager of the ticket's parenting project
    ///</summary>
    public Task<bool> ValidateProjectManagerAsync(string ticketId, string memberId);

    ///<summary>
    /// Validate if the member is an actual assigned member of the ticket
    ///</summary>
    public Task<bool> ValidateAssignedMemberAsync(string ticketId, string memberId);
    public Task<bool> ValidateAssigneeAsync(string ticketId, string memberId);
    public Task<bool> ValidateCompanyAsync(string ticketId, string? companyId);

    public Task<bool> UpdateAsigneeAsync(string ticketId, string? memberId);
    public Task<bool> UpdatePriorityAsync(string ticketId, string priority);
    public Task<bool> UpdateStatusAsync(string ticketId, string status);
    public Task<bool> UpdateTypeAsync(string ticketId, string type);
    public Task<bool> UpdateTicketAsync(string ticketId, UpdateTicketDto ticket);
    public Task<bool> ToggleArchiveTicket(string ticketId);

    public Task<List<MemberInfoDto>> GetProjectDevelopersAsync(string ticketId);
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
    string SubmitterId
);

public record UpdateTicketDto(
    string Name,
    string Description,
    string Priority,
    string Status,
    string Type
);

public record CreateTicketResult(string Id);
