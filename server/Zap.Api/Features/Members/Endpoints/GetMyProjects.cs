
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Data;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Members.Endpoints;

public class GetMyProjects : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{memberId}/myprojects", Handle)
            .WithCompanyMember();

    private static async Task<Results<Ok<List<CompanyProjectDto>>, NotFound>> Handle(
            AppDbContext db,
            CurrentUser currentUser
            )
    {
        var currentMember = await db.CompanyMembers
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

        if (currentMember == null) return TypedResults.NotFound();

        return TypedResults.Ok(currentMember.AssignedProjects);
    }
}

