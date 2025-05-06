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

    public ProjectService(AppDbContext db, ILogger<ProjectService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(string projectId)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
            .Include(p => p.ProjectManager)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.Priority,
                p.CompanyId,
                p.IsArchived,
                p.DueDate,
                p.ProjectManager == null
                ? null
                : new MemberInfoDto(
                    p.ProjectManager.Id,
                    $"{p.ProjectManager.User.FirstName} {p.ProjectManager.User.LastName}",
                    p.ProjectManager.User.AvatarUrl,
                    p.ProjectManager.Role.Name)))
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
            CompanyId = project.Member.CompanyId!
        };
        var isProjectManager = project.Member.Role?.Name == RoleNames.ProjectManager;
        if (isProjectManager)
        {
            addProject.ProjectManagerId = project.Member.Id;
        }

        var addResult = await _db.Projects.AddAsync(addProject);
        await _db.SaveChangesAsync();

        var newProject = addResult.Entity;

        MemberInfoDto? projectManager = isProjectManager == true
            ? new MemberInfoDto(
                    project.Member.Id,
                    $"{project.Member.User.FirstName} {project.Member.User.LastName}",
                    project.Member.User.AvatarUrl,
                    project.Member.Role?.Name
                    )
            : null;

        return new ProjectDto(
            newProject.Id,
            newProject.Name,
            newProject.Description,
            newProject.Priority,
            newProject.CompanyId,
            newProject.IsArchived,
            newProject.DueDate,
            projectManager);
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

    public async Task<bool> UpdateProjectManagerAsync(string projectId, string memberId)
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

    public async Task<List<MemberInfoDto>> GetAssignablePMs(string projectId)
    {
        return await _db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => new { p.CompanyId, p.ProjectManagerId })
            .SelectMany(projInfo =>
                _db.CompanyMembers
                .Where(cm =>
                    cm.CompanyId == projInfo.CompanyId &&
                    cm.Role.Name == RoleNames.ProjectManager &&
                    cm.Id != projInfo.ProjectManagerId)
                .Select(cm => new MemberInfoDto(
                    cm.Id,
                    $"{cm.User.FirstName} {cm.User.LastName}",
                    cm.User.AvatarUrl,
                    cm.Role.Name)))
            .ToListAsync();
    }
}
