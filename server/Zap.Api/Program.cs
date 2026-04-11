using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Zap.Api.Configuration;
using Zap.Api.Data;

var dotEnvFiles = Environment.GetEnvironmentVariable("ZAP_DOTENV_FILE")
    ?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

if (dotEnvFiles is { Length: > 0 })
    DotEnv.Fluent()
        .WithExceptions()
        .WithEnvFiles(dotEnvFiles)
        .WithOverwriteExistingVars()
        .Load();
else
    DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.AddStructuredLogging();
builder.Services.AddRequiredServices(builder.Configuration);

var app = builder.Build();
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Zap.Api.Startup");
var configuration = app.Services.GetRequiredService<IConfiguration>();

startupLogger.LogInformation("Starting Zap.Api in {Environment} environment.", app.Environment.EnvironmentName);

if (configuration.GetValue<bool>("Demo:EnableReset"))
    startupLogger.LogWarning("Demo reset endpoint is enabled.");

// Determine whether to apply EF Core migrations on startup.
// Controlled by the environment variable `APPLY_MIGRATIONS`.
// If the variable is not set, default to applying migrations during Development or Testing
// environments and skip by default in Production to avoid unintended schema changes.
var applyMigrationsEnv = Environment.GetEnvironmentVariable("APPLY_MIGRATIONS");
var applyMigrations = false;
if (!string.IsNullOrWhiteSpace(applyMigrationsEnv))
    bool.TryParse(applyMigrationsEnv, out applyMigrations);
else
    applyMigrations = app.Environment.IsDevelopment() ||
                      string.Equals(app.Environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase);

if (applyMigrations)
{
    // Apply any pending EF Core migrations at startup. This runs inside a service scope
    // so scoped services (like AppDbContext) can be resolved. Failures are logged but
    // won't crash the host to avoid blocking diagnostic access.
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            if (db.Database.IsRelational())
            {
                db.Database.Migrate();
                startupLogger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                startupLogger.LogInformation("Non-relational database provider detected; skipping migrations.");
            }
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex, "An error occurred while applying database migrations.");
        }
    }
}
else
{
    // Keep a startup log entry so operators can tell whether migrations were intentionally skipped.
    startupLogger.LogInformation(
        "Automatic database migrations skipped for {Environment} environment.",
        app.Environment.EnvironmentName);
}

// Seed test data in Development environment only.
// This creates a test user (test@test.com) with a complete company, members, projects, and tickets
// for development and testing purposes. Seeding is idempotent - it will skip if the test user already exists.
if (app.Environment.IsDevelopment())
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            await TestDataSeeder.SeedTestDataAsync(services, startupLogger, app.Environment);
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex, "An error occurred while seeding test data.");
        }
    }

if (app.Environment.IsDevelopment())
{
    startupLogger.LogInformation(
        "Development-only API features enabled: OpenAPI, Scalar, /auth/signin-test, /company/testmembers, and test data seeding.");
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseRequiredServices();

// Lightweight health endpoint used by orchestrators/load-balancers.
app.MapGet("/health", async (AppDbContext db, HttpContext context, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("Zap.Api.Health");

    try
    {
        // Simple connectivity check
        var canConnect = await db.Database.CanConnectAsync();
        if (canConnect) return Results.Ok("Healthy");

        logger.LogWarning("Database health check reported unhealthy status. TraceId: {TraceId}",
            context.TraceIdentifier);
        return Results.StatusCode(503);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database health check failed. TraceId: {TraceId}", context.TraceIdentifier);
        return Results.StatusCode(503);
    }
}).AllowAnonymous();

app.MapZapApiEndpoints();

app.Run();

public partial class Program
{
}
