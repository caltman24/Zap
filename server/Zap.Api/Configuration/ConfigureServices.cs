using System.Globalization;
using System.Threading.RateLimiting;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.Features.Companies.Services;
using Zap.Api.Features.Demo.Services;
using Zap.Api.Features.FileUpload.Configuration;
using Zap.Api.Features.FileUpload.Services;
using Zap.Api.Features.Projects.Services;
using Zap.Api.Features.Tickets.Services;
using Zap.Api.Features.Users.Services;

namespace Zap.Api.Configuration;

public static class ConfigureServices
{
    public static IServiceCollection AddRequiredServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi()
            .AddRateLimiting()
            .AddDataAccess(configuration)
            .AddIdentityManagement()
            .AddAuthService()
            .AddCorsPolicies()
            .AddS3Storage(configuration)
            .AddCurrentUser()
            .AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }

    private static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseRequiredSeeding();
        });


        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IDemoEnvironmentService, DemoEnvironmentService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();
        services.AddScoped<IRecentActivityService, RecentActivityService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketAuthorizationService, TicketAuthorizationService>();
        services.AddScoped<ITicketHistoryService, TicketHistoryService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();

        services.AddScoped<ITicketCommentsService, TicketCommentsService>();

        return services;
    }

    private static IServiceCollection AddIdentityManagement(this IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager<SignInManager<AppUser>>();

        return services;
    }

    private static IServiceCollection AddAuthService(this IServiceCollection services)
    {
        services.AddAuthentication(IdentityConstants.BearerScheme)
            .AddBearerToken(IdentityConstants.BearerScheme);

        services.AddAuthorizationBuilder()
            .AddCurrentUserHandler()
            .AddDefaultPolicy("default", pb =>
            {
                pb.RequireCurrentUser();
                pb.Build();
            })
            .AddFallbackPolicy("fallback", pb =>
            {
                pb.RequireCurrentUser();
                pb.Build();
            });

        return services;
    }

    private static IServiceCollection AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(opts =>
        {
            opts.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5173", "https://client.scalar.com")
                    .WithHeaders("Content-Type", "Authorization")
                    .WithMethods("GET", "POST", "PUT", "DELETE");
            });
        });

        return services;
    }

    private static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configuration)
    {
        var s3Section = configuration.GetSection("S3");
        var region = s3Section.GetValue<string>(nameof(S3Options.Region))
                     ?? throw new InvalidOperationException("Missing S3:Region configuration.");

        var awsOptions = new AWSOptions
        {
            Region = RegionEndpoint.GetBySystemName(region)
        };

        var profile = Environment.GetEnvironmentVariable("AWS_PROFILE");
        if (!string.IsNullOrWhiteSpace(profile)) awsOptions.Profile = profile;

        services.AddAWSService<IAmazonS3>(awsOptions);
        services.Configure<S3Options>(s3Section);
        services.AddScoped<IFileUploadService, S3FileUploadService>();
        return services;
    }

    private static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opts.OnRejected = (context, _) =>
            {
                context.HttpContext.Items["RateLimitRejected"] = true;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
                    context.HttpContext.Items["RetryAfterSeconds"] = retryAfterSeconds;
                    context.HttpContext.Response.Headers.RetryAfter =
                        retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
                }

                return ValueTask.CompletedTask;
            };

            // permit 10 requests per minute by user (identity) or globally:
            opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                return RateLimitPartition.GetTokenBucketLimiter(
                    context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    partition => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 90, // Burst of 90 requests
                        QueueLimit = 0,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(30),
                        TokensPerPeriod = 30, // 30 requests per 30 seconds. 60rpm sustained
                        AutoReplenishment = true
                    });
            });


            opts.AddPolicy("upload", context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5,
                        QueueLimit = 1,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
        });
        return services;
    }
}
