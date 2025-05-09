using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Projects.Services;

public sealed class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProjectService> _logger;
    private readonly ICompanyService _companyService;

    public ProjectService(AppDbContext db, ILogger<ProjectService> logger, ICompanyService companyService)
    {
        _db = db;
        _logger = logger;
        _companyService = companyService;
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(string projectId)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.Priority,
                p.CompanyId,
                p.IsArchived,
                p.DueDate,
                p.AssignedMembers.Select(m => new MemberInfoDto(
                    m.Id,
                    $"{m.User.FirstName} {m.User.LastName}",
                    m.User.AvatarUrl,
                    m.Role.Name))))
            .FirstOrDefaultAsync();

        return project;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto project)
    {
        // since `new List<CompanyMember> { project.Member }` inserts everything as new,
        // we need the member attatched to the db context
        _db.CompanyMembers.Attach(project.Member);


        var addProject = new Project
        {
            Name = project.Name,
            Description = project.Description,
            Priority = project.Priority,
            DueDate = project.DueDate,
            CompanyId = project.Member.CompanyId!,
            AssignedMembers = new List<CompanyMember> { project.Member }
        };

        var addResult = await _db.Projects.AddAsync(addProject);
        await _db.SaveChangesAsync();

        var newProject = addResult.Entity;

        return new ProjectDto(
            newProject.Id,
            newProject.Name,
            newProject.Description,
            newProject.Priority,
            newProject.CompanyId,
            newProject.IsArchived,
            newProject.DueDate,
            newProject.AssignedMembers.Select(m =>
                new MemberInfoDto(
                    m.Id,
                    $"{m.User.FirstName} {m.User.LastName}",
                    m.User.AvatarUrl,
                    m.Role.Name)));
    }

    public async Task DeleteProjectByIdAsync(string projectId)
    {
        await _db.Projects.Where(p => p.Id == projectId).ExecuteDeleteAsync();
    }

    public async Task<bool> ToggleArchiveProjectAsync(string projectId)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return false;

        project.IsArchived = !project.IsArchived;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateProjectByIdAsync(string projectId, UpdateProjectDto projectDto)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return false;

        project.Name = projectDto.Name;
        project.Description = projectDto.Description;
        project.Priority = projectDto.Priority;
        project.DueDate = projectDto.DueDate;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ValidateProjectManagerAsync(string projectId, string memberId)
    {
        return await _db.Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.AssignedMembers)
                .AnyAsync(am => am.Id == memberId && am.Role.Name == RoleNames.ProjectManager);
    }

    public async Task<SortedDictionary<string, List<MemberInfoDto>>?> GetUnassignedMembersAsync(string projectId)
    {
        var project = await _db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => new
            {
                Company = new
                {
                    Members = p.Company.Members.Select(m => new
                    {
                        m.Id,
                        User = new
                        {
                            m.User.AvatarUrl,
                            m.User.FirstName,
                            m.User.LastName,
                        },
                        RoleName = m.Role.Name
                    }).ToList()
                },
                AssignedMembers = p.AssignedMembers.Select(am => new
                {
                    am.Id,
                    User = new
                    {
                        am.User.AvatarUrl,
                        am.User.FirstName,
                        am.User.LastName,
                    },
                    RoleName = am.Role.Name
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (project == null) return null;

        var unassignedMembers = project.Company.Members
            .Where(m => !project.AssignedMembers.Select(am => am.Id).Contains(m.Id));

        var membersByRole = new SortedDictionary<string, List<MemberInfoDto>>();

        foreach (var member in unassignedMembers)
        {
            var roleName = member.RoleName;
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

    public async Task<bool> AddMembersToProjectAsync(string projectId, IEnumerable<string> memberIds)
    {
        var project = await _db.Projects
            .Where(p => p.Id == projectId)
            .Include(p => p.AssignedMembers)
            .FirstOrDefaultAsync();
        if (project == null) return false;

        try
        {
            var members = await _db.CompanyMembers.Where(m => memberIds.Contains(m.Id)).ToListAsync();
            foreach (var member in members)
            {
                project.AssignedMembers.Add(member);
            }
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding members to project");
            return false;
        }
        return true;
    }

    public async Task<bool> RemoveMemberFromProjectAsync(string projectId, string memberId)
    {
        bool removeResult = false;

        var project = await _db.Projects
            .Where(p => p.Id == projectId)
            .Include(p => p.AssignedMembers)
            .FirstOrDefaultAsync();

        if (project == null) return removeResult;

        try
        {
            var member = await _db.CompanyMembers.FirstOrDefaultAsync(m => m.Id == memberId);
            if (member == null) return removeResult;

            removeResult = project.AssignedMembers.Remove(member);
            await _db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing member from project");
            return false;
        }

        return removeResult;
    }
}
