using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Companies.Endpoints;

public class GetCompanyInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/info", Handle)
            .WithCompanyMember();

    private static async Task<Results<BadRequest<string>, Ok<CompanyInfoDto>>> Handle(
        CurrentUser currentUser,
        ICompanyService companyService,
        ILogger<Program> logger)
    {
        var companyInfo = await companyService.GetCompanyInfoAsync(currentUser.CompanyId!);
        if (companyInfo == null) return TypedResults.BadRequest("Failed to get company info");

        return TypedResults.Ok(companyInfo);
    }
}
