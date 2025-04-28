using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;
using Zap.Api.Data.Models;

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
            .Where(p => p.Id == projectId)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.Priority,
                p.CompanyId,
                p.IsArchived,
                p.DueDate))
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
            CompanyId = project.User.CompanyId!
        };

        var addResult = await _db.Projects.AddAsync(addProject);
        await _db.SaveChangesAsync();

        var newProject = addResult.Entity;

        return new ProjectDto(newProject.Id, newProject.Name,
            newProject.Description, newProject.Priority, newProject.CompanyId, newProject.IsArchived,
            newProject.DueDate);
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

}
