using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Zap.DataAccess.Services;

public sealed class CompanyService : ICompanyService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CompanyService> _logger;
    private readonly IFileUploadService _fileUploadService;

    public CompanyService(AppDbContext db, ILogger<CompanyService> logger, IFileUploadService fileUploadService)
    {
        _db = db;
        _logger = logger;
        _fileUploadService = fileUploadService;
    }

    public async Task<CompanyInfoDto?> GetCompanyInfoAsync(string companyId)
    {
        var company = await _db.Companies
            .Include(c => c.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company == null) return null;


        var membersByRole = new Dictionary<string, List<MemberInfoDto>>();

        var memberIds = company.Members.Select(m => m.Id).ToList();

        var rolesLookup = await GetMembersPerRoleAsync(memberIds);

        foreach (var member in company.Members)
        {
            var roleName = rolesLookup.GetValueOrDefault(member.Id, "None");

            if (!membersByRole.TryGetValue(roleName, out var memberList))
            {
                memberList = [];
                membersByRole[roleName] = memberList;
            }

            memberList.Add(new MemberInfoDto(
                $"{member.FirstName} {member.LastName}",
                member.AvatarUrl
            ));
        }

        return new CompanyInfoDto(company.Name,
            company.Description,
            company.LogoUrl,
            membersByRole);
    }

    public async Task<bool> UpdateCompanyInfoAsync(string companyId, string name, string description,
        string? websiteUrl,
        IFormFile? logo,
        bool removeLogo)
    {
        var company = await _db.Companies.FindAsync(companyId);
        if (company == null) return false;

        if (removeLogo && company.LogoKey != null)
        {
            _logger.LogInformation("User removing company logo {LogoKey}", company.LogoKey);
            try
            {
                await _fileUploadService.DeleteFileAsync(company.LogoKey!);
                company.LogoUrl = null;
                company.LogoKey = null;
                _logger.LogInformation("User successfully removed company logo {LogoKey}",
                company.LogoKey);
            }
            catch
            {
                return false;
            }
        }
        else if (logo != null)
        {
            _logger.LogInformation("User uploading company logo, {FileName}", logo.FileName);
            try
            {
                // Upload file
                (company.LogoUrl, company.LogoKey) =
                    await _fileUploadService.UploadCompanyLogoAsync(logo, company.LogoKey);
                _logger.LogInformation("User successfully uploaded company logo {LogoKey}",
                    company.LogoKey);
            }
            catch
            {
                return false;
            }
        }

        company.Name = name;
        company.Description = description;
        company.WebsiteUrl = websiteUrl;

        await _db.SaveChangesAsync();
        
        return true;
    }

    public async Task<List<CompanyProjectDto>> GetCompanyProjectsAsync(string companyId, bool isArchived)
    {
        return await _db.Projects
            .Where(p => p.CompanyId == companyId)
            .Where(p => p.IsArchived == isArchived)
            .Select(p => new CompanyProjectDto(
                p.Id,
                p.Name,
                p.Priority,
                p.DueDate,
                p.AssignedMembers.Count,
                p.AssignedMembers.Select(m => m.AvatarUrl).Take(5)))
            .ToListAsync();
    }
    
    public async Task<List<CompanyProjectDto>> GetAllCompanyProjectsAsync(string companyId)
    {
        return await _db.Projects
            .Where(p => p.CompanyId == companyId)
            .Select(p => new CompanyProjectDto(
                p.Id,
                p.Name,
                p.Priority,
                p.DueDate,
                p.AssignedMembers.Count,
                p.AssignedMembers.Select(m => m.AvatarUrl).Take(5)))
            .ToListAsync();
    }

    /// <summary>
    /// Gets each member from each role in the company
    /// </summary>
    /// <param name="memberIds">List of member id's</param>
    /// <returns>Dictionary of member ids (Key) to role (Value)</returns>
    private async Task<Dictionary<string, string>> GetMembersPerRoleAsync(List<string> memberIds)
    {
        return await _db.UserRoles
            .Where(ur => memberIds.Contains(ur.UserId))
            .Join(_db.Roles,
                userRole => userRole.RoleId,
                identityRole => identityRole.Id,
                (userRole, identityRole) => new { userRole.UserId, Role = identityRole.Name })
            .GroupBy(ur => ur.UserId)
            .Select(g => new { UserId = g.Key, Role = g.Select(x => x.Role).FirstOrDefault() ?? "None" })
            .ToDictionaryAsync(ur => ur.UserId, ur => ur.Role);
    }
}