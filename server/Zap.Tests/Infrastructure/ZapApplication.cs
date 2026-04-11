using System.Net.Http.Headers;
using dotenv.net;
using Microsoft.Extensions.Configuration;

namespace Zap.Tests.Infrastructure;

public class ZapApplication : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _configurationOverrides;
    private readonly bool _useInMemoryDatabase;

    public ZapApplication(Dictionary<string, string?>? configurationOverrides = null, bool useInMemoryDatabase = true)
    {
        _configurationOverrides = configurationOverrides ?? new Dictionary<string, string?>();
        _useInMemoryDatabase = useInMemoryDatabase;
    }

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
        {
            Id = id,
            Email = fallbackEmail,
            UserName = fallbackEmail,
            FirstName = "John",
            LastName = "Doe",
            EmailConfirmed = true
        };
        user.SetDefaultAvatar();
        var result = await userManager.CreateAsync(user, password ?? Guid.NewGuid().ToString());
        // if (role != null)
        // {
        //     var res = await userManager.AddToRoleAsync(user, role);
        //     Assert.True(res.Succeeded);
        // }
        Assert.True(result.Succeeded);
    }

    public new HttpClient CreateClient()
    {
        return base.CreateClient();
    }

    public HttpClient CreateClient(string id, string role = RoleNames.Admin)
    {
        return CreateDefaultClient(new AuthHandler(Services, id, role));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        if (_configurationOverrides.Count > 0)
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(_configurationOverrides);
            });

        base.ConfigureWebHost(builder);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Overwrite .env file from api
        DotEnv.Fluent()
            .WithExceptions()
            .WithEnvFiles(".env")
            .WithOverwriteExistingVars()
            .Load();
        builder.ConfigureServices((context, services) =>
        {
            // DbContext
            services.AddDbContextFactory<AppDbContext>();
            services.AddDbContextOptions(context.Configuration, _useInMemoryDatabase);

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