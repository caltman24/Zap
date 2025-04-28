using Zap.Api.Data.Models;

namespace Zap.Api.Features.Companies.Services;

public interface ICompanyService
{
    Task<CompanyInfoDto?> GetCompanyInfoAsync(string companyId);

    Task<bool> UpdateCompanyInfoAsync(UpdateCompanyInfoDto updateCompanyDto);

    Task<List<CompanyProjectDto>> GetCompanyProjectsAsync(string companyId, bool isArchived);
    Task<List<CompanyProjectDto>> GetAllCompanyProjectsAsync(string companyId);
    Task CreateCompanyAsync(CreateCompanyDto company);
    Task DeleteCompanyByIdAsync(string companyId);
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
    Dictionary<string, List<MemberInfoDto>> Members);

public record MemberInfoDto(string Id, string Name, string AvatarUrl, string? Role = null);

public record CompanyProjectDto(
    string Id,
    string Name,
    string Priority,
    DateTime DueDate,
    bool IsArchived,
    IEnumerable<string> AvatarUrls);

