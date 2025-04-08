using System.Net.Http.Headers;


namespace Zap.Tests;

public class ZapApplication : WebApplicationFactory<Program>
{
    public AppDbContext CreateAppDbContext()
    {
        var db = Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
        db.Database.EnsureCreated();

        return db;
    }

    public async Task CreateUserAsync(string id, string? email = null, string? password = null)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var fallbackEmail = email ?? id + "@test.com";
        var user = new AppUser
            { Id = id, Email = fallbackEmail, UserName = fallbackEmail, FirstName = "John", LastName = "Doe", EmailConfirmed = true};
        user.SetDefaultAvatar();
        var result = await userManager.CreateAsync(user, password ?? Guid.NewGuid().ToString());

        Assert.True(result.Succeeded);
    }

    public HttpClient CreateClient(string id, string role = RoleNames.Admin)
    {
        return CreateDefaultClient(new AuthHandler(Services, id, role));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Since we use .env file in the api, we manually set the env variables here
        // Currently aren't testing s3, so this is just to successfully run the tests
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
            services.AddDbContextFactory<AppDbContext>();
            services.AddDbContextOptions();

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

            // Since tests run in parallel, it's possible multiple servers will startup,
            // we use an ephemeral key provider and repository to avoid filesystem contention issues
            services.AddSingleton<IDataProtectionProvider, EphemeralDataProtectionProvider>();

            services.AddScoped<TokenService>();
        });
        return base.CreateHost(builder);
    }

    private sealed class AuthHandler(IServiceProvider services, string id, string role) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await using var scope = services.CreateAsyncScope();

            // Generate tokens
            var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();

            var token = await tokenService.GenerateTokenAsync(id, role);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}