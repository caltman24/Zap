using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Extensions;
using Zap.DataAccess;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;
using Zap.DataAccess.Services;

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

        // Update company info with multipart form data.
        // file can be null
        // can also remove the image
        group.MapPost("/info",
                async Task<Results<BadRequest<string>, NoContent>> (HttpContext context, IFileUploadService fileUploadService, UserManager<AppUser> userManager,
                    AppDbContext db) =>
                {
                    if (!context.Request.HasFormContentType)
                    {
                        return TypedResults.BadRequest("Invalid content type.  " +
                                                  "Must be multipart/form-data.");
                    }

                    var (file, upsertCompanyInfoRequest) = context.ParseMultipartForm<UpsertCompanyInfoRequest>();

                    var user = await userManager.GetUserAsync(context.User);
                    if (user == null) return TypedResults.BadRequest("User not found");

                    var company = await db.Companies.FindAsync(user.CompanyId);
                    if (company == null) return TypedResults.BadRequest("Company not found");
                    if (upsertCompanyInfoRequest.RemoveLogo)
                    {
                        company.LogoUrl = null;
                    }

                    if (file != null)
                    {
                        company.LogoUrl = await fileUploadService.UploadAvatarAsync(file);
                    }

                    company.Name = upsertCompanyInfoRequest.Name;
                    company.Description = upsertCompanyInfoRequest.Description;
                    company.WebsiteUrl = upsertCompanyInfoRequest.WebsiteUrl;

                    db.Companies.Update(company);
                    await db.SaveChangesAsync();

                    return TypedResults.NoContent();
                })
            .RequireAuthorization(pb => { pb.RequireRole(RoleNames.Admin); });

        return app;
    }
}

public record MembersResponse(string Name, string AvatarUrl);

public record CompanyInfoResponse(string Name, string Description, Dictionary<string, List<MembersResponse>> Members);

public record UpsertCompanyInfoRequest(string Name, string Description, bool RemoveLogo, string? WebsiteUrl);