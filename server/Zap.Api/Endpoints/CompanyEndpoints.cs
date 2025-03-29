using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Authorization;
using Zap.DataAccess;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;
using Zap.DataAccess.Services;

namespace Zap.Api.Endpoints;

internal static class CompanyEndpoints
{
    internal static IEndpointRouteBuilder MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/company");

        group.MapGet("/info", GetCompanyInfoHandler);
        group.MapPost("/info", PostCompanyInfoHandler)
            .DisableAntiforgery()
            .Accepts<UpsertCompanyInfoRequest>("multipart/form-data")
            .RequireRateLimiting("upload")
            .RequireAuthorization(pb => { pb.RequireRole(RoleNames.Admin); });

        var projectsGroup = group.MapGroup("/projects");
        projectsGroup.MapGet("/", GetCompanyProjectsHandler);

        return app;
    }

    private static async Task<Results<BadRequest<string>, Ok<CompanyInfoResponse>>> GetCompanyInfoHandler(
        AppDbContext db,
        CurrentUser currentUser,
        ILogger<Program> logger)
    {
        var user = currentUser.User;
        if (user == null) return TypedResults.BadRequest("User not found");

        var company = await db.Companies
            .Include(c => c.Members)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == user.CompanyId);

        if (company == null) return TypedResults.BadRequest("Company not found");

        var membersByRole = new Dictionary<string, List<MembersResponse>>();

        var memberIds = company.Members.Select(m => m.Id).ToList();

        var rolesLookup = await db.UserRoles
            .Where(ur => memberIds.Contains(ur.UserId))
            .Join(db.Roles,
                userRole => userRole.RoleId,
                identityRole => identityRole.Id,
                (userRole, identityRole) => new { userRole.UserId, Role = identityRole.Name })
            .GroupBy(ur => ur.UserId)
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
    }

    private static async Task<Results<BadRequest<string>, NoContent, ProblemHttpResult>> PostCompanyInfoHandler(
        [FromForm] IFormFile? file,
        [FromForm] UpsertCompanyInfoRequest upsertCompanyInfoRequest,
        HttpContext context,
        IFileUploadService fileUploadService,
        CurrentUser currentUser,
        ILogger<Program> logger, AppDbContext db)
    {
        if (currentUser.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var company = await db.Companies.FindAsync(currentUser.CompanyId);
        if (company == null) return TypedResults.BadRequest("Company not found");

        if (upsertCompanyInfoRequest.RemoveLogo && company.LogoKey != null)
        {
            logger.LogInformation("User {Email} removing company logo {LogoKey}", currentUser.Email, company.LogoKey);
            try
            {
                await fileUploadService.DeleteFileAsync(company.LogoKey!);
                company.LogoUrl = null;
                company.LogoKey = null;
                logger.LogInformation("User {Email} successfully removed company logo {LogoKey}", currentUser.Email,
                    company.LogoKey);
            }
            catch (Exception ex)
            {
                return TypedResults.Problem("Failed to delete company logo", statusCode: 500);
            }
        }
        else if (file != null)
        {
            logger.LogInformation("User {Email} uploading company logo", currentUser.Email);
            try
            {
                // Validate file
                if (company.LogoKey != null)
                {
                    await fileUploadService.DeleteFileAsync(company.LogoKey);
                }

                // Upload file
                (company.LogoUrl, company.LogoKey) = await fileUploadService.UploadCompanyLogoAsync(file);
                logger.LogInformation("User {Email} successfully uploaded company logo {LogoKey}", currentUser.Email,
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
    }

    private static async Task<Results<BadRequest<string>, Ok<List<CompanyProjectsResponse>>>> GetCompanyProjectsHandler(
        AppDbContext db, CurrentUser currentUser, ILogger<Program> logger)
    {
        var user = currentUser.User;
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var projects = await db.Projects
            .Where(p => p.CompanyId == user.CompanyId)
            .Select(p => new CompanyProjectsResponse(
                p.Id,
                p.Name,
                p.Priority,
                p.DueDate,
                p.AssignedMembers.Count,
                p.AssignedMembers.Select(m => m.AvatarUrl).Take(5)))
            .ToListAsync();

        return TypedResults.Ok(projects);
    }
}

record MembersResponse(string Name, string AvatarUrl);

record CompanyInfoResponse(
    string Name,
    string Description,
    string? LogoUrl,
    Dictionary<string, List<MembersResponse>> Members);

record UpsertCompanyInfoRequest(string Name, string Description, bool RemoveLogo, string? WebsiteUrl);

record CompanyProjectsResponse(
    string Id,
    string Name,
    string Priority,
    DateTime DueDate,
    int MemberCount,
    IEnumerable<string> AvatarUrls);