﻿using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Projects.Services;

public interface IProjectService
{
    Task<ProjectDto?> GetProjectByIdAsync(string projectId);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto project);
    Task DeleteProjectByIdAsync(string projectId);
    // returns false if already archived
    Task<bool> ToggleArchiveProjectAsync(string projectId);
    // returns false if already unarchived
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
