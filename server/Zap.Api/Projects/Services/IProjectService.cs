using Zap.Api.Companies.Services;
using Zap.Api.Data.Models;

namespace Zap.Api.Projects.Services;

public interface IProjectService
{
    Task<ProjectDto?> GetProjectByIdAsync(string projectId);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto project);
    Task DeleteProjectByIdAsync(string projectId);
    // returns false if already archived
    Task<bool> ArchiveProjectAsync(string projectId);
    // returns false if already unarchived
    Task<bool> UnarchiveProjectAsync(string projectId);
}

public record CreateProjectDto(string Name, string Description, string Priority, DateTime DueDate, AppUser User);

public record ProjectDto(
    string Id,
    string Name,
    string Description,
    string Priority,
    string CompanyId,
    bool IsArchived,
    DateTime DueDate,
    IEnumerable<MemberInfoDto> Members);
