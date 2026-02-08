using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Zap.Api.Configuration;
using Zap.Api.Data;

var builder = WebApplication.CreateBuilder(args);
DotEnv.Load();

builder.Logging.AddStructuredLogging(builder.Configuration);
builder.Services.AddRequiredServices(builder.Configuration);

var app = builder.Build();

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
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            if (db.Database.IsRelational())
            {
                db.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Non-relational database provider detected; skipping migrations.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
        }
    }
}
else
{
    // Keep a startup log entry so operators can tell whether migrations were intentionally skipped.
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation(
        "APPLY_MIGRATIONS is false or unset for this environment; skipping automatic database migrations.");
}

// Seed test data in Development environment only.
// This creates a test user (test@test.com) with a complete company, members, projects, and tickets
// for development and testing purposes. Seeding is idempotent - it will skip if the test user already exists.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            await TestDataSeeder.SeedTestDataAsync(services, logger, app.Environment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding test data.");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

app.UseRequiredServices();

// Lightweight health endpoint used by orchestrators/load-balancers.
app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        // Simple connectivity check
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect ? Results.Ok("Healthy") : Results.StatusCode(503);
    }
    catch
    {
        return Results.StatusCode(503);
    }
}).AllowAnonymous();

app.MapZapApiEndpoints();

app.Run();

public partial class Program
{
}