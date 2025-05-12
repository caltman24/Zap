using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Projects.Services;

public interface IProjectService
{
    Task<ProjectDto?> GetProjectByIdAsync(string projectId);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto project);
    Task DeleteProjectByIdAsync(string projectId);
    // returns false if already archived
    Task<bool> ToggleArchiveProjectAsync(string projectId);
    Task<bool> UpdateProjectByIdAsync(string projectId, UpdateProjectDto projectDto);

    Task<bool> UpdateProjectManagerAsync(string projectId, string? memberId);
    Task<bool> ValidateProjectManagerAsync(string projectId, string memberId);
    Task<List<ProjectManagerDto>> GetAssignablePMs(string projectId);

    // Assign Members
    Task<SortedDictionary<string, List<MemberInfoDto>>?> GetUnassignedMembersAsync(string projectId, string memberId);
    Task<bool> AddMembersToProjectAsync(string projectId, IEnumerable<string> memberIds);
    Task<bool> RemoveMemberFromProjectAsync(string projectId, string memberId);
}

public record CreateProjectDto(string Name, string Description, string Priority, DateTime DueDate, CompanyMember Member);
public record UpdateProjectDto(string Name, string Description, string Priority, DateTime DueDate);
public record ProjectManagerDto(string Id, string Name, string AvatarUrl, string Role, bool Assigned);

public record ProjectDto(
    string Id,
    string Name,
    string Description,
    string Priority,
    string CompanyId,
    MemberInfoDto? ProjectManager,
    bool IsArchived,
    DateTime DueDate,
    IEnumerable<BasicTicketDto> Tickets,
    IEnumerable<MemberInfoDto> Members);
