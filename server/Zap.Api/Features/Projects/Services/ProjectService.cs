using Microsoft.EntityFrameworkCore;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Projects.Services;

public sealed class ProjectService : IProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db)
    {
        _db = db;
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
            project.AssignedMembers.Select(m => new MemberInfoDto($"{m.FirstName} {m.LastName}", m.AvatarUrl)));
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
                new MemberInfoDto($"{m.FirstName} {m.LastName}", m.AvatarUrl)));
    }

    public async Task DeleteProjectByIdAsync(string projectId)
    {
        await _db.Projects.Where(p => p.Id == projectId).ExecuteDeleteAsync();
    }

    public async Task<bool> ArchiveProjectAsync(string projectId)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null || project.IsArchived == true) return false;

        project.IsArchived = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnarchiveProjectAsync(string projectId)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null || project.IsArchived == false) return false;

        project.IsArchived = false;
        await _db.SaveChangesAsync();
        return true;
    }
}