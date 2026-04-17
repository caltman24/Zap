using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.FileUpload.Services;

namespace Zap.Api.Features.Companies.Services;

public sealed class CompanyService : ICompanyService
{
    private const float NamePrefixScore = 350;
    private const float NameTokenPrefixBaseScore = 300;
    private const float NameTokenPrefixRankMultiplier = 100;

    private readonly AppDbContext _db;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(AppDbContext db, IFileUploadService fileUploadService, ILogger<CompanyService> logger)
    {
        _db = db;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public async Task<CompanyInfoDto?> GetCompanyInfoAsync(string companyId)
    {
        var company = await _db.Companies
            .Where(c => c.Id == companyId)
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Include(c => c.Members).ThenInclude(m => m.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (company == null) return null;

        // Where string is the role name
        var membersByRole = GetMembersPerRole(company.Members);

        return new CompanyInfoDto(company.Name,
            company.Description,
            company.LogoUrl,
            membersByRole);
    }

    public async Task<bool> UpdateCompanyInfoAsync(UpdateCompanyInfoDto updateCompanyDto)
    {
        var company = await _db.Companies.FindAsync(updateCompanyDto.CompanyId);
        if (company == null) return false;

        try
        {
            if (updateCompanyDto.RemoveLogo && company.LogoKey != null)
            {
                await _fileUploadService.DeleteFileAsync(company.LogoKey);
                company.LogoUrl = null;
                company.LogoKey = null;
            }
            else if (updateCompanyDto.Logo != null)
            {
                // Upload file
                (company.LogoUrl, company.LogoKey) =
                    await _fileUploadService.UploadCompanyLogoAsync(updateCompanyDto.Logo, company.LogoKey);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error uploading company logo to s3");
            return false;
        }

        company.Name = updateCompanyDto.Name;
        company.Description = updateCompanyDto.Description;
        company.WebsiteUrl = updateCompanyDto.WebsiteUrl;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<List<ProjectSearchDto>> SearchVisibleProjectsAsync(
        string companyId,
        string memberId,
        string roleName,
        string searchTerm,
        int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || limit <= 0) return [];

        var trimmedSearchTerm = searchTerm.Trim();
        var namePrefixPattern = $"{trimmedSearchTerm}%";
        var nameTokenPrefixQuery = BuildNameTokenPrefixQuery(trimmedSearchTerm);

        if (nameTokenPrefixQuery == null) return [];

        var query = _db.Projects
            .AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .Where(p => !p.IsArchived);

        query = roleName switch
        {
            RoleNames.Admin => query,
            RoleNames.ProjectManager => query.Where(p => p.ProjectManagerId == memberId),
            RoleNames.Developer => query.Where(p => p.AssignedMembers.Any(m => m.Id == memberId)),
            RoleNames.Submitter => query.Where(p => p.AssignedMembers.Any(m => m.Id == memberId)),
            _ => query.Where(_ => false)
        };

        return await query
            .Select(project => new
            {
                project.Id,
                project.Name,
                NameVector = EF.Functions.ToTsVector("simple", project.Name)
            })
            .Select(project => new
            {
                project.Id,
                project.Name,
                NamePrefixMatch = EF.Functions.ILike(project.Name, namePrefixPattern),
                NameTokenPrefixMatch =
                    project.NameVector.Matches(EF.Functions.ToTsQuery("simple", nameTokenPrefixQuery)),
                NameTokenPrefixRank = project.NameVector.Rank(EF.Functions.ToTsQuery("simple", nameTokenPrefixQuery))
            })
            .Select(project => new
            {
                Score =
                    (project.NamePrefixMatch ? NamePrefixScore : 0) +
                    (project.NameTokenPrefixMatch
                        ? NameTokenPrefixBaseScore + project.NameTokenPrefixRank * NameTokenPrefixRankMultiplier
                        : 0),
                project.Id,
                project.Name
            })
            .Where(project => project.Score > 0)
            .OrderByDescending(project => project.Score)
            .ThenBy(project => project.Name)
            .Take(limit)
            .Select(project => new ProjectSearchDto(project.Id, project.Name)
            {
                Score = project.Score
            })
            .ToListAsync();
    }

    public async Task<List<CompanyProjectDto>> GetVisibleProjectsAsync(
        string companyId,
        string memberId,
        string roleName,
        bool? isArchived)
    {
        var query = _db.Projects.Where(p => p.CompanyId == companyId);

        if (isArchived.HasValue)
            query = query.Where(p => p.IsArchived == isArchived.Value);

        query = roleName switch
        {
            RoleNames.Admin => query,
            RoleNames.ProjectManager => query.Where(p => p.ProjectManagerId == memberId),
            RoleNames.Developer => query.Where(p => p.AssignedMembers.Any(m => m.Id == memberId)),
            RoleNames.Submitter => query.Where(p => p.AssignedMembers.Any(m => m.Id == memberId)),
            _ => query.Where(_ => false)
        };

        var result = await ProjectSummaryQuery(query).ToListAsync();

        return ProjectSummaryResult(result);
    }

    public async Task<List<CompanyProjectDto>> GetAllCompanyProjectsAsync(string companyId, bool isArchived)
    {
        var result = await ProjectSummaryQuery(_db.Projects
                .Where(p => p.CompanyId == companyId)
                .Where(p => p.IsArchived == isArchived))
            .ToListAsync();

        return ProjectSummaryResult(result);
    }

    public async Task CreateCompanyAsync(CreateCompanyDto company)
    {
        var newCompany = new Company
        {
            Name = company.Name,
            Description = company.Description,
            OwnerId = company.User.Id
        };

        var roleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.Admin).Select(r => r.Id).FirstAsync();
        var newMember = new CompanyMember
        {
            UserId = company.User.Id,
            CompanyId = newCompany.Id,
            RoleId = roleId
        };
        newCompany.Members.Add(newMember);

        // await _db.CompanyMembers.AddAsync(newMember);
        await _db.Companies.AddAsync(newCompany);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCompanyByIdAsync(string companyId)
    {
        await _db.Companies.Where(c => c.Id == companyId).ExecuteDeleteAsync();
    }

    /// <summary>
    ///     Gets each member from each role in the company
    /// </summary>
    /// <param name="memberIds">List of member id's</param>
    /// <returns>Dictionary of member ids (Key) to role (Value)</returns>
    public SortedDictionary<string, List<MemberInfoDto>> GetMembersPerRole(IEnumerable<CompanyMember> companyMembers)
    {
        var membersByRole = new SortedDictionary<string, List<MemberInfoDto>>();

        foreach (var member in companyMembers)
        {
            var roleName = member.Role.Name;
            if (!membersByRole.TryGetValue(roleName, out var memberList))
            {
                memberList = [];
                membersByRole[roleName] = memberList;
            }

            memberList.Add(new MemberInfoDto(
                member.Id,
                $"{member.User.FirstName} {member.User.LastName}",
                member.User.AvatarUrl,
                roleName
            ));
        }

        return membersByRole;
    }

    public async Task<string?> GetMemberRoleAsync(string memberId)
    {
        return await _db.CompanyMembers
            .Where(m => m.Id == memberId)
            .Include(m => m.Role)
            .Select(m => m.Role.Name)
            .FirstOrDefaultAsync();
    }

    private static string? BuildNameTokenPrefixQuery(string searchTerm)
    {
        var tokens = searchTerm
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeSearchToken)
            .Where(token => token.Length > 0)
            .Select(token => $"{token}:*")
            .ToList();

        return tokens.Count == 0
            ? null
            : string.Join(" & ", tokens);
    }

    private static string NormalizeSearchToken(string searchTerm)
    {
        return new string(searchTerm
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());
    }

    private static List<CompanyProjectDto> ProjectSummaryResult(IEnumerable<ProjectSummaryRow> result)
    {
        return
        [
            .. result.Select(p =>
            {
                if (p.ProjectManagerAvatar != null) p.AvatarUrls.Add(p.ProjectManagerAvatar);

                return new CompanyProjectDto(
                    p.Id,
                    p.Name,
                    p.Priority,
                    p.DueDate,
                    p.IsArchived,
                    p.MemberCount,
                    p.AvatarUrls
                );
            })
        ];
    }

    private static IQueryable<ProjectSummaryRow> ProjectSummaryQuery(IQueryable<Project> query)
    {
        return query
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Priority,
                p.DueDate,
                p.IsArchived,
                MemberCount = p.ProjectManagerId != null
                    ? p.AssignedMembers.Count() + 1
                    : p.AssignedMembers.Count(),
                AvatarUrls = p.AssignedMembers.Select(m => m.User.AvatarUrl).Take(5).ToList(),
                ProjectManagerAvatar = p.ProjectManager != null ? p.ProjectManager.User.AvatarUrl : null
            })
            .Select(p => new ProjectSummaryRow(
                p.Id,
                p.Name,
                p.Priority,
                p.DueDate,
                p.IsArchived,
                p.MemberCount,
                p.AvatarUrls,
                p.ProjectManagerAvatar));
    }

    private sealed record ProjectSummaryRow(
        string Id,
        string Name,
        string Priority,
        DateTime DueDate,
        bool IsArchived,
        int MemberCount,
        List<string> AvatarUrls,
        string? ProjectManagerAvatar);
}
