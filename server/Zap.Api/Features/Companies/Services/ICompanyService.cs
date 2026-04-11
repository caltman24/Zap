using Zap.Api.Data.Models;

namespace Zap.Api.Features.Companies.Services;

public interface ICompanyService
{
    Task<CompanyInfoDto?> GetCompanyInfoAsync(string companyId);

    Task<bool> UpdateCompanyInfoAsync(UpdateCompanyInfoDto updateCompanyDto);

    /// <summary>
    ///     Searches active projects visible to the current member.
    /// </summary>
    /// <param name="companyId">The current company identifier.</param>
    /// <param name="memberId">The current member identifier.</param>
    /// <param name="roleName">The current member role name.</param>
    /// <param name="searchTerm">The raw search term entered by the user.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <returns>A list of visible active projects matching the search term.</returns>
    Task<List<ProjectSearchDto>> SearchVisibleProjectsAsync(
        string companyId,
        string memberId,
        string roleName,
        string searchTerm,
        int limit = 5);

    Task<List<CompanyProjectDto>> GetVisibleProjectsAsync(string companyId, string memberId, string roleName,
        bool? isArchived);

    Task<List<CompanyProjectDto>> GetAllCompanyProjectsAsync(string companyId, bool isArchived);
    Task CreateCompanyAsync(CreateCompanyDto company);
    Task DeleteCompanyByIdAsync(string companyId);
    Task<string?> GetMemberRoleAsync(string memberId);
    SortedDictionary<string, List<MemberInfoDto>> GetMembersPerRole(IEnumerable<CompanyMember> companyMembers);
}

public record CreateCompanyDto(string Name, string Description, AppUser User);

public record UpdateCompanyInfoDto(
    string CompanyId,
    string Name,
    string Description,
    string? WebsiteUrl,
    IFormFile? Logo,
    bool RemoveLogo);

public record CompanyInfoDto(
    string Name,
    string Description,
    string? LogoUrl,
    SortedDictionary<string, List<MemberInfoDto>> Members);

public record MemberInfoDto(string Id, string Name, string AvatarUrl, string? Role = null);

public record CompanyProjectDto(
    string Id,
    string Name,
    string Priority,
    DateTime DueDate,
    bool IsArchived,
    int MemberCount,
    IEnumerable<string> AvatarUrls);

public record ProjectSearchDto(string Id, string Name)
{
    public float Score { get; init; }
}