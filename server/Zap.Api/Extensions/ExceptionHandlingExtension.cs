using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Zap.Api.Extensions;

public static class ExceptionHandlingExtension
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, ILogger logger)
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