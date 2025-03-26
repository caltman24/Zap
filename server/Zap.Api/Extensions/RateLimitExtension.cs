using System.Threading.RateLimiting;

namespace Zap.Api.Extensions;

public static class RateLimitExtension
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            // permit 10 requests per minute by user (identity) or globally:
            opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                return RateLimitPartition.GetTokenBucketLimiter(
                    context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    partition => new TokenBucketRateLimiterOptions()
                    {
                        TokenLimit = 100,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 20,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                        TokensPerPeriod = 20, // 120rpm sustained
                        AutoReplenishment = true
                    });
            });


            opts.AddPolicy("upload", context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    partition => new FixedWindowRateLimiterOptions()
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
        });
        return services;
    }
}