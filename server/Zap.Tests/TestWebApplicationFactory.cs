using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Internal;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.Util.Internal;
using dotenv.net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Zap.DataAccess.Configuration;
using Zap.DataAccess.Models;
using Zap.DataAccess.Services;

namespace Zap.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public AppDbContext CreateAppDbContext()
    {
        var db = Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
        db.Database.EnsureCreated();

        return db;
    }

    public async Task CreateUserAsync(string username, string? password = null)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = new AppUser { Id = username, UserName = username, FirstName = "John", LastName = "Doe" };
        user.SetDefaultAvatar();
        var result = await userManager.CreateAsync(user, password ?? Guid.NewGuid().ToString());

        Assert.True(result.Succeeded);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("AWS_S3_BUCKET", "Development");
        Environment.SetEnvironmentVariable("AWS_PROFILE", "Development");
        Environment.SetEnvironmentVariable("AWS_REGION", "Development");
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY", "Development");
        Environment.SetEnvironmentVariable("AWS_SECRET_KEY", "Development");

        base.ConfigureWebHost(builder);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // DbContext

            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.AddDbContextFactory<AppDbContext>(o => { o.UseInMemoryDatabase("Zap.Tests");o.UseInternalServiceProvider(Services); });

            // Lower the requirements for the tests
            services.Configure<IdentityOptions>(o =>
            {
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireDigit = false;
                o.Password.RequiredUniqueChars = 0;
                o.Password.RequiredLength = 1;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
            });
        });
        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        var dbContext = CreateAppDbContext();
        dbContext.Database.EnsureDeleted();
        base.Dispose(disposing);
    }
}