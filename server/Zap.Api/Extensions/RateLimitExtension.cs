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
                        TokenLimit = 20, // Burst of 20 requests
                        QueueLimit = 0,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(30),
                        TokensPerPeriod = 10, // 10 requests per 30 seconds. 20rpm sustained
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