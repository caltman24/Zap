using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Projects.Services;

public sealed class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(AppDbContext db, ILogger<ProjectService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(string projectId)
    {
        var project = await _db.Projects
            .Include(p => p.AssignedMembers)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return null;

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description,
            project.Priority,
            project.CompanyId,
            project.IsArchived,
            project.DueDate,
            project.AssignedMembers.Select(m => new MemberInfoDto(m.Id, $"{m.FirstName} {m.LastName}", m.AvatarUrl)));
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto project)
    {
        var addProject = new Project
        {
            Name = project.Name,
            Description = project.Description,
            Priority = project.Priority,
            DueDate = project.DueDate,
            CompanyId = project.User.CompanyId!,
            AssignedMembers = new List<AppUser> { project.User },
        };

        var addResult = await _db.Projects.AddAsync(addProject);
        await _db.SaveChangesAsync();

        var newProject = addResult.Entity;

        return new ProjectDto(newProject.Id, newProject.Name,
            newProject.Description, newProject.Priority, newProject.CompanyId, newProject.IsArchived,
            newProject.DueDate,
            newProject.AssignedMembers.Select(m =>
                new MemberInfoDto(m.Id, $"{m.FirstName} {m.LastName}", m.AvatarUrl)));
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

    public async Task<Dictionary<string, List<MemberInfoDto>>?> GetUnassignedMembersAsync(string projectId)
    {
        var project = await _db.Projects
            .Where(p => p.Id == projectId)
            .Include(p => p.Company)
            .ThenInclude(c => c.Members)
            .Include(p => p.AssignedMembers)
            .FirstOrDefaultAsync();

        if (project == null) return null;

        var unassignedMembers = project.Company.Members
            .Where(m => !project.AssignedMembers.Select(am => am.Id).Contains(m.Id));

        var membersByRole = new Dictionary<string, List<MemberInfoDto>>();

        if (unassignedMembers.Count() == 0) return membersByRole;

        // userId: RoleName
        Dictionary<string, string> rolesLookup = await _db.UserRoles
            .Where(ur => unassignedMembers.Select(um => um.Id).Contains(ur.UserId))
            .Join(_db.Roles,
                userRole => userRole.RoleId,
                identityRole => identityRole.Id,
                (userRole, identityRole) => new { userRole.UserId, Role = identityRole.Name })
            .GroupBy(ur => ur.UserId)
            .Select(g => new { UserId = g.Key, Role = g.Select(x => x.Role).FirstOrDefault() ?? "None" })
            .ToDictionaryAsync(ur => ur.UserId, ur => ur.Role);


        foreach (var member in unassignedMembers)
        {
            var roleName = rolesLookup.GetValueOrDefault(member.Id, "None");

            if (!membersByRole.TryGetValue(roleName, out var memberList))
            {
                memberList = [];
                membersByRole[roleName] = memberList;
            }

            memberList.Add(new MemberInfoDto(
                member.Id,
                $"{member.FirstName} {member.LastName}",
                member.AvatarUrl,
                roleName
            ));
        }

        //TODO: Sort the members alpha
        return membersByRole.OrderBy(kvp => kvp.Key)
            .ToDictionary();
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
            var users = await _db.Users.Where(u => memberIds.Contains(u.Id)).ToListAsync();
            foreach (var user in users)
            {
                project.AssignedMembers.Add(user);
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
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == memberId);
            if (user == null) return removeResult;

            removeResult = project.AssignedMembers.Remove(user);
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
