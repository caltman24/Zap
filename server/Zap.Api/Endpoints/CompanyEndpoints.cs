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
                UserManager<AppUser> userManager, HttpContext context, ILogger<Program> logger) =>
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

                var memberIds = company.Members.Select(m => m.Id).ToList();

                var rolesLookup = await db.UserRoles
                    // Filter UserRoles
                    .Where(ur => memberIds.Contains(ur.UserId))
                    // join with Roles table
                    .Join(db.Roles, // IdentityRole
                        userRole => userRole.RoleId, // Key selector for the outer sequence (UserRoles)
                        identityRole => identityRole.Id, // Key selector for the inner sequence (Roles)
                        // Project the result of the join
                        (userRole, identityRole) => new { userRole.UserId, Role = identityRole.Name })
                    .GroupBy(ur => ur.UserId)
                    // Select the first role for each user
                    .Select(g => new { UserId = g.Key, Role = g.Select(x => x.Role).FirstOrDefault() ?? "None" })
                    .ToDictionaryAsync(ur => ur.UserId, ur => ur.Role);


                foreach (var member in company.Members)
                {
                    var roleName = rolesLookup.GetValueOrDefault(member.Id, "None");

                    if (!membersByRole.TryGetValue(roleName, out var memberList))
                    {
                        memberList = [];
                        membersByRole[roleName] = memberList;
                    }

                    memberList.Add(new MembersResponse(
                        $"{member.FirstName} {member.LastName}",
                        member.AvatarUrl
                    ));
                }

                return TypedResults.Ok(new CompanyInfoResponse(company.Name, company.Description, company.LogoUrl,
                    membersByRole));
            }).RequireAuthorization();

        // Update company info with multipart form data.
        // file can be null
        // can also remove the image
        group.MapPost("/info", async Task<Results<BadRequest<string>, NoContent, ProblemHttpResult>> (
                [FromForm] IFormFile? file,
                [FromForm] UpsertCompanyInfoRequest upsertCompanyInfoRequest,
                HttpContext context,
                IFileUploadService fileUploadService,
                UserManager<AppUser> userManager,
                ILogger<Program> logger,
                AppDbContext db) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

                var company = await db.Companies.FindAsync(user.CompanyId);
                if (company == null) return TypedResults.BadRequest("Company not found");

                if (upsertCompanyInfoRequest.RemoveLogo && company.LogoKey != null)
                {
                    logger.LogInformation("User {Email} removing company logo {LogoKey}", user.Email, company.LogoKey);
                    try
                    {
                        await fileUploadService.DeleteFileAsync(company.LogoKey!);
                        company.LogoUrl = null;
                        company.LogoKey = null;
                        logger.LogInformation("User {Email} successfully removed company logo {LogoKey}", user.Email,
                            company.LogoKey);
                    }
                    catch (Exception ex)
                    {
                        return TypedResults.Problem("Failed to delete company logo", statusCode: 500);
                    }
                }
                else if (file != null)
                {
                    logger.LogInformation("User {Email} uploading company logo", user.Email);
                    try
                    {
                        // Validate file
                        if (company.LogoKey != null)
                        {
                            await fileUploadService.DeleteFileAsync(company.LogoKey);
                        }

                        // Upload file
                        (company.LogoUrl, company.LogoKey) = await fileUploadService.UploadCompanyLogoAsync(file);
                        logger.LogInformation("User {Email} successfully uploaded company logo {LogoKey}", user.Email,
                            company.LogoKey);
                    }
                    catch (Exception ex)
                    {
                        return TypedResults.Problem("Failed to upload company logo", statusCode: 500);
                    }
                }

                company.Name = upsertCompanyInfoRequest.Name;
                company.Description = upsertCompanyInfoRequest.Description;
                company.WebsiteUrl = upsertCompanyInfoRequest.WebsiteUrl;

                await db.SaveChangesAsync();

                return TypedResults.NoContent();
            }).DisableAntiforgery()
            .Accepts<UpsertCompanyInfoRequest>("multipart/form-data")
            .RequireRateLimiting("upload")
            .RequireAuthorization(pb => { pb.RequireRole(RoleNames.Admin); });

        return app;
    }
}

public record MembersResponse(string Name, string AvatarUrl);

public record CompanyInfoResponse(
    string Name,
    string Description,
    string? LogoUrl,
    Dictionary<string, List<MembersResponse>> Members);

public record UpsertCompanyInfoRequest(string Name, string Description, bool RemoveLogo, string? WebsiteUrl);