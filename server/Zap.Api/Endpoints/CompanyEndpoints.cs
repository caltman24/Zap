using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.DataAccess;
using Zap.DataAccess.Models;

namespace Zap.Api.Endpoints;

public static class CompanyEndpoints
{
    public static IEndpointRouteBuilder MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/company");

        group.MapGet("/info",
            async Task<Results<BadRequest<string>, Ok<CompanyInfoResponse>>> (AppDbContext db,
                UserManager<AppUser> userManager, HttpContext context) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return TypedResults.BadRequest("User not found");

                // Load company with members in a single query
                var company = await db.Companies
                    .Include(c => c.Members)
                    .FirstOrDefaultAsync(c => c.Id == user.CompanyId);
                
                if (company == null) return TypedResults.BadRequest("Company not found");

                var memberResponses = new List<MembersResponse>();
                foreach (var member in company.Members)
                {
                    var roles = await userManager.GetRolesAsync(member);
                    memberResponses.Add(new MembersResponse($"{member.FirstName} {member.LastName}", roles.FirstOrDefault() ?? "User"));
                }

                return TypedResults.Ok(new CompanyInfoResponse(company.Name, company.Description, memberResponses));
            }).RequireAuthorization();

        return app;
    }
}

public record MembersResponse(string Name, string Role);

public record CompanyInfoResponse(string Name, string Description, IEnumerable<MembersResponse> Members);