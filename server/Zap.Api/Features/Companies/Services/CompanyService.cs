﻿using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
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
                new List<string>()))
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
                new List<string>()))
            .ToListAsync();
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
    /// Gets each member from each role in the company
    /// </summary>
    /// <param name="memberIds">List of member id's</param>
    /// <returns>Dictionary of member ids (Key) to role (Value)</returns>
    public SortedDictionary<string, List<MemberInfoDto>> GetMembersPerRole(ICollection<CompanyMember> companyMembers)
    {
        var membersByRole = new SortedDictionary<string, List<MemberInfoDto>>();

        foreach (var member in companyMembers)
        {
            var roleName = member.Role!.Name;
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
        var member = await _db.CompanyMembers.FindAsync(memberId);

        return member?.Role.Name;
    }

}
