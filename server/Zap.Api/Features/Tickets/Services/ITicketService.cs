using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketService
{
    public Task<CreateTicketResult> CreateTicketAsync(CreateTicketDto ticket, string creatorId);
    public Task<BasicTicketDto?> GetTicketByIdAsync(string ticketId);
    public Task<List<BasicTicketDto>> GetAssignedTicketsAsync(string memberId);
    public Task<List<BasicTicketDto>> GetOpenTicketsAsync(string memberId, string roleName, string companyId);
    public Task<List<BasicTicketDto>> GetArchivedTicketsAsync(string memberId, string roleName, string companyId);
    public Task<List<BasicTicketDto>> GetResolvedTicketsAsync(string memberId, string roleName, string companyId);
    public Task DeleteTicketAsync(string ticketId);

    /// <summary>
    ///     Validate if the member is the actual project manager of the ticket's parenting project
    /// </summary>
    public Task<bool> ValidateProjectManagerAsync(string ticketId, string memberId);

    /// <summary>
    ///     Validate if the member is an actual assigned member of the ticket
    /// </summary>
    public Task<bool> ValidateAssignedMemberAsync(string ticketId, string memberId);

    public Task<bool> ValidateAssigneeAsync(string ticketId, string memberId);
    public Task<bool> ValidateCompanyAsync(string ticketId, string? companyId);

    public Task<bool> UpdateAsigneeAsync(string ticketId, string? memberId, string updaterId);
    public Task<bool> UpdatePriorityAsync(string ticketId, string priority, string updaterId);
    public Task<bool> UpdateStatusAsync(string ticketId, string status, string updaterId);
    public Task<bool> UpdateTypeAsync(string ticketId, string type, string updaterId);
    public Task<bool> UpdateTicketAsync(string ticketId, UpdateTicketDto ticket, string updaterId);
    public Task<bool> UpdateArchivedTicketAsync(string ticketId, string name, string description, string updaterId);
    public Task<bool> ToggleArchiveTicket(string ticketId, string updaterId);

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
    string? ProjectManagerId,
    bool isArchived,
    bool ProjectIsArchived,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    MemberInfoDto Submitter,
    MemberInfoDto? Assignee
)
{
    public string DisplayId => FormatDisplayId(Id);

    public static string FormatDisplayId(string ticketId)
    {
        var compactId = ticketId.Replace("-", string.Empty);
        var suffix = compactId.Length >= 4
            ? compactId[^4..]
            : compactId.PadLeft(4, '0');

        return $"#ZAP-{suffix.ToUpperInvariant()}";
    }
}

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
