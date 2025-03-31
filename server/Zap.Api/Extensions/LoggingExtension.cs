using Serilog;

namespace Zap.Api.Extensions;

public static class LoggingExtension
{
    public static void AddStructuredLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        builder.ClearProviders();
        builder.AddSerilog(new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger());
    }
}