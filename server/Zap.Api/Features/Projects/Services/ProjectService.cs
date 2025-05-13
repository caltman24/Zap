using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Tickets.Services;

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
            .Where(p => p.Id == projectId)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.Priority,
                p.CompanyId,
                p.ProjectManager == null
                ? null
                : new MemberInfoDto(
                    p.ProjectManager.Id,
                    $"{p.ProjectManager.User.FirstName} {p.ProjectManager.User.LastName}",
                    p.ProjectManager.User.AvatarUrl,
                    p.ProjectManager.Role.Name),
                p.IsArchived,
                p.DueDate,
                p.Tickets.Select(t => new BasicTicketDto(
                    t.Id,
                    t.Name,
                    t.Description,
                    t.Priority.Name,
                    t.Status.Name,
                    t.Type.Name,
                    t.ProjectId,
                    new MemberInfoDto(
                        t.Submitter.Id,
                        $"{t.Submitter.User.FirstName} {t.Submitter.User.LastName}",
                        t.Submitter.User.AvatarUrl,
                        t.Submitter.Role.Name),
                    t.Assignee == null
                        ? null
                        : new MemberInfoDto(
                        t.Assignee.Id,
                        $"{t.Assignee.User.FirstName} {t.Assignee.User.LastName}",
                        t.Assignee.User.AvatarUrl,
                        t.Assignee.Role.Name)
                )),
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
        var addProject = new Project
        {
            Name = project.Name,
            Description = project.Description,
            Priority = project.Priority,
            DueDate = project.DueDate,
            CompanyId = project.Member.CompanyId!,
            AssignedMembers = new List<CompanyMember> { }
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
            newProject.ProjectManager == null
                ? null
                : new MemberInfoDto(
                    newProject.ProjectManager.Id,
                    $"{newProject.ProjectManager.User.FirstName} {newProject.ProjectManager.User.LastName}",
                    newProject.ProjectManager.User.AvatarUrl,
                    newProject.ProjectManager.Role.Name),
            newProject.IsArchived,
            newProject.DueDate,
            new List<BasicTicketDto> { },
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
        var rowsChanged = await _db.Projects
            .Where(p => p.Id == projectId)
            .ExecuteUpdateAsync(setter => setter.SetProperty(s => s.IsArchived, s => !s.IsArchived));

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateProjectByIdAsync(string projectId, UpdateProjectDto projectDto)
    {
        var rowsChanged = await _db.Projects
            .Where(p => p.Id == projectId)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Name, projectDto.Name)
                    .SetProperty(p => p.Description, projectDto.Description)
                    .SetProperty(p => p.Priority, projectDto.Priority)
                    .SetProperty(p => p.DueDate, projectDto.DueDate));

        return rowsChanged > 0;
    }

    public async Task<bool> UpdateProjectManagerAsync(string projectId, string? memberId)
    {
        int rowsChanged = await _db.Projects
            .Where(p => p.Id == projectId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.ProjectManagerId, memberId));

        return rowsChanged > 0;
    }

    public async Task<bool> ValidateProjectManagerAsync(string projectId, string memberId)
    {
        return await _db.Projects.AnyAsync(p => p.Id == projectId && p.ProjectManagerId == memberId);
    }

    public async Task<List<ProjectManagerDto>> GetAssignablePMs(string projectId)
    {
        return await _db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => new { p.CompanyId, p.ProjectManagerId })
            .SelectMany(projInfo =>
                _db.CompanyMembers
                .Where(cm =>
                    cm.CompanyId == projInfo.CompanyId &&
                    cm.Role.Name == RoleNames.ProjectManager)
                .Select(cm => new ProjectManagerDto(
                    cm.Id,
                    $"{cm.User.FirstName} {cm.User.LastName}",
                    cm.User.AvatarUrl,
                    cm.Role.Name,
                    cm.Id == projInfo.ProjectManagerId)))
            .ToListAsync();
    }

    public async Task<SortedDictionary<string, List<MemberInfoDto>>?> GetUnassignedMembersAsync(string projectId, string memberId)
    {
        var project = await _db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => new
            {
                Company = new
                {
                    Members = p.Company.Members
                        .Where(m => m.Id != memberId)
                        .Where(m => m.Role.Name == RoleNames.Submitter || m.Role.Name == RoleNames.Developer)
                        .Select(m => new
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

    public async Task<List<BasicProjectDto>> GetAssignedProjects(string memberId, string roleName, string companyId)
    {

        if (roleName == RoleNames.ProjectManager)
        {
            return await _db.Projects
                .Where(p => p.ProjectManagerId == memberId)
                .Select(p => new BasicProjectDto(p.Id, p.Name))
                .ToListAsync();
        }

        if (roleName == RoleNames.Admin)
        {
            return await _db.Projects
                .Where(p => p.CompanyId == companyId)
                .Select(p => new BasicProjectDto(p.Id, p.Name))
                .ToListAsync();
        }

        return await _db.CompanyMembers
            .Where(cm => cm.Id == memberId)
            .SelectMany(cm => cm.AssignedProjects)
            .Select(p => new BasicProjectDto(p.Id, p.Name))
            .ToListAsync();

    }
}
