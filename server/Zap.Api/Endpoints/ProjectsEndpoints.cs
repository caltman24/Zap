using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Authorization;
using Zap.DataAccess;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

internal static class ProjectsEndpoints
{
    internal static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects");
        group.MapGet("/{ProjectId}", GetProjectHandler)
            .WithName("GetProject");

        group.MapPost("/", CreateProjectHandler)
            .RequireAuthorization(pb =>
            {
                pb.RequireRole(RoleNames.Admin, RoleNames.ProjectManager)
                    .RequireCurrentUser()
                    .Build();
            });

        return app;
    }

    private static async Task<Results<BadRequest<string>, Ok<ProjectResponse>>> GetProjectHandler(
        [FromRoute] string projectId, AppDbContext db, CurrentUser currentUser, ILogger<Program> logger)
    {
        var user = currentUser.User;
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var project = await db.Projects
            .Include(p => p.AssignedMembers)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return TypedResults.BadRequest("Project not found");

        // If the project is not in the same company as the user
        if (project.CompanyId != user.CompanyId) return TypedResults.BadRequest("Project not found");

        // Only admins can see all projects
        // if (currentUser.Role != RoleNames.Admin && !project.AssignedMembers.Contains(user))
        //     return TypedResults.BadRequest("Project not found");

        var response = new ProjectResponse(project.Id, project.Name, project.Description, project.Priority,
            project.IsArchived, project.DueDate, project.AssignedMembers.Select(m =>
                new MemberResponse($"{m.FirstName} {m.LastName}", m.AvatarUrl)));

        return TypedResults.Ok(response);
    }

    private static async Task<Results<BadRequest<string>, CreatedAtRoute<ProjectResponse>>> CreateProjectHandler(
        CreateProjectRequest request, AppDbContext db, CurrentUser currentUser, ILogger<Program> logger)
    {
        var user = currentUser.User;
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            CompanyId = user.CompanyId,
            AssignedMembers = new List<AppUser> { user },
        };

        var addResult = await db.Projects.AddAsync(project);
        await db.SaveChangesAsync();

        var newProject = addResult.Entity;

        var response = new ProjectResponse(newProject.Id, newProject.Name,
            newProject.Description, newProject.Priority, newProject.IsArchived, newProject.DueDate,
            newProject.AssignedMembers.Select(m =>
                new MemberResponse($"{m.FirstName} {m.LastName}", m.AvatarUrl)));

        return TypedResults.CreatedAtRoute(response, "GetProject", new { ProjectId = newProject.Id });
    }
}

record CreateProjectRequest(string Name, string Description, string Priority, DateTime DueDate);

record MemberResponse(string Name, string AvatarUrl);

record ProjectResponse(
    string Id,
    string Name,
    string Description,
    string Priority,
    bool IsArchived,
    DateTime DueDate,
    IEnumerable<MemberResponse> Members);