using Microsoft.AspNetCore.Http.HttpResults;
using Zap.Api.Common;
using Zap.Api.Common.Authorization;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Api.Features.Tickets;

public class GetRecentActivity : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/recent-activity", Handle)
            .WithName("GetRecentActivity")
            .WithCompanyMember();
    }

    private static async Task<Ok<List<RecentActivityDto>>> Handle(
        IRecentActivityService recentActivityService,
        CurrentUser currentUser)
    {
        var activity = await recentActivityService.GetRecentActivityAsync(currentUser);
        return TypedResults.Ok(activity);
    }
}