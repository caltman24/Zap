using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using Serilog.Events;
using Zap.Api.Common.Authorization;

namespace Zap.Api.Configuration;

public static class ConfigureApp
{
    public static void UseRequiredServices(this WebApplication app)
    {
        app.UseGlobalExceptionHandler(app.Services.GetRequiredService<ILoggerFactory>());
        app.UseStructuredRequestLogging();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
    }

    private static IApplicationBuilder UseStructuredRequestLogging(this WebApplication app)
    {
        var configuration = app.Services.GetRequiredService<IConfiguration>();
        var slowRequestThresholdMs = configuration.GetValue("Observability:SlowRequestThresholdMs",
            app.Environment.IsDevelopment() ? 500d : 1000d);

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "Request completed for {RequestMethod} {RequestPath} with {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsedMs, exception) =>
                GetRequestLogLevel(httpContext, elapsedMs, exception, app.Environment, slowRequestThresholdMs);
            options.EnrichDiagnosticContext = EnrichDiagnosticContext;
        });

        return app;
    }

    private static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Zap.Api.ExceptionHandler");

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

                    var currentUser = context.RequestServices.GetService<CurrentUser>();

                    logger.LogError(contextFeature.Error,
                        "Unhandled exception occurred while processing {RequestMethod} {RequestPath}. StatusCode: {StatusCode}. TraceId: {TraceId}. UserId: {UserId}. CompanyId: {CompanyId}",
                        context.Request.Method,
                        context.Request.Path.Value,
                        context.Response.StatusCode,
                        context.TraceIdentifier,
                        context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                        currentUser?.CompanyId);

                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        context.Response.StatusCode,
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

    private static LogEventLevel GetRequestLogLevel(HttpContext httpContext, double elapsedMs, Exception? exception,
        IWebHostEnvironment environment, double slowRequestThresholdMs)
    {
        if (IsQuietHealthCheck(httpContext, exception)) return LogEventLevel.Verbose;

        if (exception != null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode == StatusCodes.Status429TooManyRequests ||
            elapsedMs >= slowRequestThresholdMs ||
            IsSecuritySensitiveAuthFailure(httpContext, environment))
            return LogEventLevel.Warning;

        return environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Verbose;
    }

    private static void EnrichDiagnosticContext(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);

        if (httpContext.GetEndpoint()?.DisplayName is { Length: > 0 } endpointName)
            diagnosticContext.Set("EndpointName", endpointName);

        if (Activity.Current?.Id is { Length: > 0 } activityId)
            diagnosticContext.Set("ActivityId", activityId);

        if (httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } userId)
            diagnosticContext.Set("UserId", userId);

        if (httpContext.RequestServices.GetService<CurrentUser>()?.CompanyId is { Length: > 0 } companyId)
            diagnosticContext.Set("CompanyId", companyId);

        if (httpContext.Items.TryGetValue("RateLimitRejected", out var rateLimitRejected) &&
            rateLimitRejected is true)
            diagnosticContext.Set("RateLimitRejected", true);

        if (httpContext.Items.TryGetValue("RetryAfterSeconds", out var retryAfterSeconds) &&
            retryAfterSeconds is int retryAfter)
            diagnosticContext.Set("RetryAfterSeconds", retryAfter);
    }

    private static bool IsQuietHealthCheck(HttpContext httpContext, Exception? exception)
    {
        return exception == null &&
               httpContext.Response.StatusCode < StatusCodes.Status400BadRequest &&
               httpContext.Request.Path.StartsWithSegments("/health");
    }

    private static bool IsSecuritySensitiveAuthFailure(HttpContext httpContext, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment()) return false;

        if (httpContext.Response.StatusCode is not (StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden))
            return false;

        return httpContext.Request.Path.StartsWithSegments("/auth") ||
               httpContext.Request.Path.StartsWithSegments("/demo");
    }
}
