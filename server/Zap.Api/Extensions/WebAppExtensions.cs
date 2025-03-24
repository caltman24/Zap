using Zap.Api.Endpoints;

namespace Zap.Api.Extensions;

public static class WebAppExtensions
{
    public static void MapZapApiEndpoints(this WebApplication app)
    {
        app.MapRegisterUserEndpoints()
            .MapSignInEndpoints()
            .MapUserEndpoints()
            .MapCompanyEndpoints();
    }
}