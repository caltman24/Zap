
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Data;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Members.Endpoints;

public class GetMyProjects : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{memberId}/myprojects", Handle)
            .WithCompanyMember(RoleNames.Submitter, RoleNames.Developer, RoleNames.ProjectManager);

    private static async Task<Results<Ok<List<CompanyProjectDto>>, NotFound>> Handle(
            AppDbContext db,
            CurrentUser currentUser
            )
    {
        dynamic? currentMember = null;

        if (currentUser.Role == RoleNames.ProjectManager)
        {
            currentMember = await db.Projects
                .Where(p => p.ProjectManagerId == currentUser.Member!.Id)
                .Select(p => new
                {
                    AssignedProjects =
                        new CompanyProjectDto(
                           p.Id,
                           p.Name,
                           p.Priority,
                           p.DueDate,
                           p.IsArchived,
                           p.AssignedMembers.Count(),
                           p.AssignedMembers.Select(m => m.User.AvatarUrl).Take(5).ToList())
                })
                .FirstOrDefaultAsync();
        }
        else
        {
            currentMember = await db.CompanyMembers
               .Where(cm => cm.Id == currentUser.Member!.Id)
               .Select(cm => new
               {
                   AssignedProjects = cm.AssignedProjects
                       .Select(ap => new CompanyProjectDto(
                           ap.Id,
                           ap.Name,
                           ap.Priority,
                           ap.DueDate,
                           ap.IsArchived,
                           ap.AssignedMembers.Count(),
                           ap.AssignedMembers.Select(m => m.User.AvatarUrl).Take(5).ToList())).ToList()
               }).FirstOrDefaultAsync();
        }


        if (currentMember == null) return TypedResults.NotFound();

        return TypedResults.Ok(currentMember.AssignedProjects);
    }
}

