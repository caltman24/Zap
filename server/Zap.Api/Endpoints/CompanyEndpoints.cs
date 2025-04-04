﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Authorization;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Services;

namespace Zap.Api.Endpoints;

public static class CompanyEndpoints
{
    public static IEndpointRouteBuilder MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/company");

        group.MapGet("/info", GetCompanyInfoHandler);
        group.MapPost("/info", PostCompanyInfoHandler)
            .DisableAntiforgery()
            .Accepts<UpdateCompanyInfoRequest>("multipart/form-data")
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
        [FromForm] UpdateCompanyInfoRequest updateCompanyInfoRequest,
        HttpContext context,
        ICompanyService companyService,
        CurrentUser currentUser,
        ILogger<Program> logger)
    {
        if (currentUser.CompanyId == null) return TypedResults.BadRequest("User not in company");

        var success = await companyService.UpdateCompanyInfoAsync(new UpdateCompanyInfoDto(
            currentUser.CompanyId,
            updateCompanyInfoRequest.Name,
            updateCompanyInfoRequest.Description,
            updateCompanyInfoRequest.WebsiteUrl,
            file,
            updateCompanyInfoRequest.RemoveLogo));

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

public record UpdateCompanyInfoRequest(string Name, string Description, bool RemoveLogo, string? WebsiteUrl);