using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.FileUpload.Services;

namespace Zap.Api.Features.Companies.Services;

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

        // Where string is the role name
        Dictionary<string, List<MemberInfoDto>> membersByRole = await GetMembersPerRoleAsync(company.Members);

        return new CompanyInfoDto(company.Name,
            company.Description,
            company.LogoUrl,
            membersByRole);
    }

    public async Task<bool> UpdateCompanyInfoAsync(UpdateCompanyInfoDto updateCompanyDto)
    {
        var company = await _db.Companies.FindAsync(updateCompanyDto.CompanyId);
        if (company == null) return false;

        if (updateCompanyDto.RemoveLogo && company.LogoKey != null)
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
        else if (updateCompanyDto.Logo != null)
        {
            _logger.LogInformation("User uploading company logo, {FileName}", updateCompanyDto.Logo.FileName);
            try
            {
                // Upload file
                (company.LogoUrl, company.LogoKey) =
                    await _fileUploadService.UploadCompanyLogoAsync(updateCompanyDto.Logo, company.LogoKey);
                _logger.LogInformation("User successfully uploaded company logo {LogoKey}",
                    company.LogoKey);
            }
            catch
            {
                return false;
            }
        }

        company.Name = updateCompanyDto.Name;
        company.Description = updateCompanyDto.Description;
        company.WebsiteUrl = updateCompanyDto.WebsiteUrl;

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
                p.IsArchived,
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
                p.IsArchived,
                p.AssignedMembers.Count,
                p.AssignedMembers.Select(m => m.AvatarUrl).Take(5)))
            .ToListAsync();
    }

    public async Task CreateCompanyAsync(CreateCompanyDto company)
    {
        var newCompany = new Company
        {
            Name = company.Name,
            Description = company.Description,
            OwnerId = company.User.Id,
            Members = new List<AppUser> { company.User },
        };

        await _db.Companies.AddAsync(newCompany);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCompanyByIdAsync(string companyId)
    {
        await _db.Companies.Where(c => c.Id == companyId).ExecuteDeleteAsync();
    }

    public async Task<Dictionary<string, List<MemberInfoDto>>?> GetCompanyMembersPerRoleAsync(string companyId)
    {
        var company = await _db.Companies
                .Where(c => c.Id == companyId)
                .Include(c => c.Members)
                .AsNoTracking()
                .FirstOrDefaultAsync();

        if (company == null) return null;

        var membersPerRole = await GetMembersPerRoleAsync(company.Members);

        return membersPerRole;
    }

    /// <summary>
    /// Gets each member from each role in the company
    /// </summary>
    /// <param name="memberIds">List of member id's</param>
    /// <returns>Dictionary of member ids (Key) to role (Value)</returns>
    public async Task<Dictionary<string, List<MemberInfoDto>>> GetMembersPerRoleAsync(ICollection<AppUser> companyMembers)
    {
        var memberIds = companyMembers.Select(m => m.Id).ToList();

        var rolesLookup = await _db.UserRoles
            .Where(ur => memberIds.Contains(ur.UserId))
            .Join(_db.Roles,
                userRole => userRole.RoleId,
                identityRole => identityRole.Id,
                (userRole, identityRole) => new { userRole.UserId, Role = identityRole.Name })
            .GroupBy(ur => ur.UserId)
            .Select(g => new { UserId = g.Key, Role = g.Select(x => x.Role).FirstOrDefault() ?? "None" })
            .ToDictionaryAsync(ur => ur.UserId, ur => ur.Role);

        var membersByRole = new Dictionary<string, List<MemberInfoDto>>();

        foreach (var member in companyMembers)
        {
            var roleName = rolesLookup.GetValueOrDefault(member.Id, "None");

            if (!membersByRole.TryGetValue(roleName, out var memberList))
            {
                memberList = [];
                membersByRole[roleName] = memberList;
            }

            memberList.Add(new MemberInfoDto(
                $"{member.FirstName} {member.LastName}",
                member.AvatarUrl,
                roleName
            ));
        }

        return membersByRole;
    }

}
