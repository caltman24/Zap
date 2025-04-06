using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace Zap.Api;

public static class ConfigureApp
{
    public static void UseRequiredServices(this WebApplication app)
    {
        app.UseGlobalExceptionHandler(app.Services.GetRequiredService<ILogger<Program>>());
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
    }

    private static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    // Set appropriate status code based on exception type
                    context.Response.StatusCode = contextFeature.Error switch
                    {
                        KeyNotFoundException => StatusCodes.Status404NotFound,
                        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                        // Add other exception types as needed
                        _ => StatusCodes.Status500InternalServerError
                    };

                    logger.LogError(contextFeature.Error, "Unhandled exception occurred");

                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = context.Response.StatusCode == StatusCodes.Status500InternalServerError
                            ? "An unexpected error occurred"
                            : contextFeature.Error.Message
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
            });
        });

        return app;
    }
}