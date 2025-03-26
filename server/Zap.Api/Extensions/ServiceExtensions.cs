using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using dotenv.net.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.DataAccess;
using Zap.DataAccess.Configuration;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;
using Zap.DataAccess.Services;

namespace Zap.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        return services;
    }

    public static IServiceCollection AddIdentityManagement(this IServiceCollection services)
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

    public static IServiceCollection AddAuthService(this IServiceCollection services)
    {
        services.AddAuthentication(IdentityConstants.BearerScheme)
            .AddBearerToken(IdentityConstants.BearerScheme);

        services.AddAuthorizationBuilder()
            .AddDefaultPolicy("default", pb =>
            {
                pb.RequireAuthenticatedUser();
                pb.Build();
            });

        return services;
    }

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
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

    public static IServiceCollection AddS3Storage(this IServiceCollection services)
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
}