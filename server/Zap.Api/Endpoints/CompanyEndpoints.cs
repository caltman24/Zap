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
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == user.CompanyId);

                    if (company == null) return TypedResults.BadRequest("Company not found");

                    var membersByRole = new Dictionary<string, List<MembersResponse>>();

                    foreach (var member in company.Members)
                    {
                        var roles = await userManager.GetRolesAsync(member);
                        var role = roles.FirstOrDefault() ?? "None";
                        if (!membersByRole.TryGetValue(role, out var value))
                        {
                            value = [];
                            membersByRole[role] = value;
                        }

                        value.Add(new MembersResponse(member.FirstName + " " + member.LastName, member.AvatarUrl));
                    }

                    return TypedResults.Ok(new CompanyInfoResponse(company.Name, company.Description, membersByRole));
                }).RequireAuthorization();

        return app;
    }
}

public record MembersResponse(string Name, string AvatarUrl);

public record CompanyInfoResponse(string Name, string Description, Dictionary<string, List<MembersResponse>> Members);