using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Tickets.Services;

public interface ITicketService
{
    /// <summary>
    ///     Searches active tickets visible to the current member.
    /// </summary>
    /// <param name="memberId">The current member identifier.</param>
    /// <param name="roleName">The current member role name.</param>
    /// <param name="companyId">The current company identifier.</param>
    /// <param name="searchTerm">The raw search term entered by the user.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <returns>A list of visible active tickets matching the search term.</returns>
    public Task<List<TicketSearchDto>> SearchVisibleTicketsAsync(
        string memberId,
        string roleName,
        string companyId,
        string searchTerm,
        int limit = 5);

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
    public string? StoredDisplayId { get; init; }
    public string DisplayId => StoredDisplayId ?? FormatDisplayId(Id);

    public static string FormatDisplayId(string ticketId)
    {
        var compactId = ticketId.Replace("-", string.Empty);
        var suffix = compactId.Length >= 4
            ? compactId[^4..]
            : compactId.PadLeft(4, '0');

        return $"#ZAP-{suffix.ToUpperInvariant()}";
    }
}

public record TicketSearchDto(string Id, string ProjectId, string Name)
{
    public float Score { get; init; }
    public string? StoredDisplayId { get; init; }
    public string DisplayId => StoredDisplayId ?? BasicTicketDto.FormatDisplayId(Id);
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