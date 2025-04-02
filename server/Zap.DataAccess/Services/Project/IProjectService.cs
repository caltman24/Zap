using Zap.DataAccess.Models;

namespace Zap.DataAccess.Services;

public interface IProjectService
{
    Task<ProjectDto?> GetProjectByIdAsync(string projectId);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto project);
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
