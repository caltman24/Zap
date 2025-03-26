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

        app.MapPost("/upload/avatar", [RequestSizeLimit(5 * 1024 * 1024)]
                async (IFormFile file, IFileUploadService fileUploadService) =>
                {
                    var uploadKey = await fileUploadService.UploadAvatarAsync(file);

                    return TypedResults.Ok(uploadKey);
                }).DisableAntiforgery()
            .RequireRateLimiting("upload");
    }
}