﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Companies.Endpoints;

public class UpdateCompanyInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/info", Handle)
            .DisableAntiforgery()
            .Accepts<Request>("multipart/form-data")
            .RequireRateLimiting("upload")
            .WithCompanyMember(RoleNames.Admin);

    public record Request(string Name, string Description, bool RemoveLogo, string? WebsiteUrl);

    private static async Task<Results<BadRequest<string>, NoContent, ProblemHttpResult>> Handle(
        [FromForm] IFormFile? file,
        [FromForm] Request updateCompanyInfoRequest,
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
}
