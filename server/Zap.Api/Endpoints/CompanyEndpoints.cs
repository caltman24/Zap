using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Authorization;
using Zap.DataAccess.Constants;
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

    private static async Task<Results<BadRequest<string>, Ok<CompanyInfoDto>>> GetCompanyInfoHandler(
        CurrentUser currentUser,
        ICompanyService companyService,
        ILogger<Program> logger)
    {
        var user = currentUser.User;
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");
        
        var companyInfo = await companyService.GetCompanyInfoAsync(user.CompanyId);
        if (companyInfo == null) return TypedResults.BadRequest("Failed to get company info");

        return TypedResults.Ok(companyInfo);
    }

    private static async Task<Results<BadRequest<string>, NoContent, ProblemHttpResult>> PostCompanyInfoHandler(
        [FromForm] IFormFile? file,
        [FromForm] UpsertCompanyInfoRequest upsertCompanyInfoRequest,
        HttpContext context,
        ICompanyService companyService,
        CurrentUser currentUser,
        ILogger<Program> logger)
    {
        if (currentUser.CompanyId == null) return TypedResults.BadRequest("User not in company");
            
        var success = await companyService.UpdateCompanyInfoAsync(
            currentUser.CompanyId, 
            upsertCompanyInfoRequest.Name,
            upsertCompanyInfoRequest.Description,
            upsertCompanyInfoRequest.WebsiteUrl,
            file,
            upsertCompanyInfoRequest.RemoveLogo);
        
        if (success) return TypedResults.NoContent();
        
        return TypedResults.BadRequest("Failed to update company info");
        
    }

    private static async Task<Results<BadRequest<string>, Ok<List<CompanyProjectDto>>>> GetCompanyProjectsHandler(
        ICompanyService companyService, CurrentUser currentUser, ILogger<Program> logger, [FromQuery] bool isArchived)
    {
        var user = currentUser.User;
        if (user?.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var projects = await companyService.GetCompanyProjectsAsync(user.CompanyId, isArchived);

        return TypedResults.Ok(projects);
    }
}

internal record UpsertCompanyInfoRequest(string Name, string Description, bool RemoveLogo, string? WebsiteUrl);
