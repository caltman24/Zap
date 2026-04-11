using Serilog;

namespace Zap.Api.Configuration;

public static class ConfigureLogging
{
    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

        return builder;
    }
}
