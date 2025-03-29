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
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    logger.LogError(contextFeature.Error, "Unhandled exception occurred");

                    var response = new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "An unexpected error occurred"
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
            });
        });

        return app;
    }
}