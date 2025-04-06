using System.Threading.RateLimiting;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using dotenv.net.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Authorization;
using Zap.Api.Common.Constants;
using Zap.Api.Companies.Services;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Api.FileUpload.Configuration;
using Zap.Api.FileUpload.Services;
using Zap.Api.Projects.Services;

namespace Zap.Api;

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
            .AddS3Storage()
            .AddCurrentUser()
            .AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }

    private static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.UseRoleSeeding();
        });


        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IProjectService, ProjectService>();

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
            .AddRoles<IdentityRole>()
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

    private static IServiceCollection AddS3Storage(this IServiceCollection services)
    {
        services.AddAWSService<IAmazonS3>(new AWSOptions()
        {
            Region = RegionEndpoint.GetBySystemName(EnvReader.GetStringValue("AWS_REGION")),
            Credentials = new BasicAWSCredentials(EnvReader.GetStringValue("AWS_ACCESS_KEY"),
                EnvReader.GetStringValue("AWS_SECRET_KEY")),
            Profile = EnvReader.GetStringValue("AWS_PROFILE"),
        });
        services.Configure<S3Options>(options =>
        {
            options.BucketName = EnvReader.GetStringValue("AWS_S3_BUCKET");
            options.Region = EnvReader.GetStringValue("AWS_REGION");
        });
        services.AddScoped<IFileUploadService, S3FileUploadService>();
        return services;
    }

    private static IServiceCollection AddRateLimiting(this IServiceCollection services)
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
                        PermitLimit = 5,
                        QueueLimit = 1,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });
        });
        return services;
    }
}