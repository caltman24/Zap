using Microsoft.AspNetCore.Mvc;
using Zap.Api.Endpoints;
using Zap.DataAccess.Services;

namespace Zap.Api.Extensions;

public static class WebAppExtensions
{
    public static void UseRequiredServices(this WebApplication app)
    {
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
    }

    public static void MapZapApiEndpoints(this WebApplication app)
    {
        app.MapRegisterUserEndpoints()
            .MapSignInEndpoints()
            .MapUserEndpoints()
            .MapCompanyEndpoints();
    }
}