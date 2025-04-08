using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Companies.Services;

namespace Zap.Api.Features.Companies.Endpoints;

public class GetCompanyInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/info", Handle);
    

    private static async Task<Results<BadRequest<string>, Ok<CompanyInfoDto>>> Handle(
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
}