using Microsoft.AspNetCore.Http;

namespace Zap.DataAccess.Services;

public interface ICompanyService
{
    Task<CompanyInfoDto?> GetCompanyInfoAsync(string companyId);
    Task<bool> UpdateCompanyInfoAsync(string companyId, string name, string description, string? websiteUrl, 
        IFormFile? logo, bool removeLogo);
    Task<List<CompanyProjectDto>> GetCompanyProjectsAsync(string companyId, bool isArchived);
    Task<List<CompanyProjectDto>> GetAllCompanyProjectsAsync(string companyId);
}

public record CompanyInfoDto(
    string Name,
    string Description,
    string? LogoUrl,
    Dictionary<string, List<MemberInfoDto>> Members);

public record MemberInfoDto(string Name, string AvatarUrl);

public record CompanyProjectDto(
    string Id,
    string Name,
    string Priority,
    DateTime DueDate,
    int MemberCount,
    IEnumerable<string> AvatarUrls);