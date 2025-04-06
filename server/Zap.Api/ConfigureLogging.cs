using Serilog;

namespace Zap.Api;

public static class ConfigureLogging
{
    public static void AddStructuredLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        builder.ClearProviders();
        builder.AddSerilog(new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger());
    }
}