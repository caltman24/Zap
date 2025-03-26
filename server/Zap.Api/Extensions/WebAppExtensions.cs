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

        app.MapPost("/upload", (IFormFile file) =>
        {
            using var memoryStream = new MemoryStream();
            file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            return TypedResults.Ok();
        }).DisableAntiforgery();
    }
}