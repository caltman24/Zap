using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Zap.DataAccess.Constants;
using Zap.DataAccess.Models;

namespace Zap.DataAccess;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Build configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.Development.json", optional: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Create DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        var useSeeding = configuration["UseSeeding"]?.ToLower() == "true";

        if (useSeeding)
        {
            // Called as part of EnsureCreated, Migrate, and `dotnet ef database update`
            optionsBuilder.UseSeeding((ctx, _) =>
                {
                    ctx.Set<IdentityRole>().AddRange([
                        new IdentityRole
                        {
                            Name = RoleNames.Admin,
                            NormalizedName = RoleNames.Admin.ToUpperInvariant(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        },
                        new IdentityRole
                        {
                            Name = RoleNames.ProjectManager,
                            NormalizedName = RoleNames.ProjectManager.ToUpperInvariant(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        },
                        new IdentityRole
                        {
                            Name = RoleNames.Developer,
                            NormalizedName = RoleNames.Developer.ToUpperInvariant(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        },
                        new IdentityRole
                        {
                            Name = RoleNames.Submitter,
                            NormalizedName = RoleNames.Submitter.ToUpperInvariant(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        }
                    ]);

                    ctx.SaveChanges();
                })
                .UseAsyncSeeding(async (ctx, _, ct) =>
                {
                    await ctx.Set<IdentityRole>().AddRangeAsync([
                        new IdentityRole
                        {
                            Name = RoleNames.Admin,
                            NormalizedName = RoleNames.Admin.ToUpperInvariant(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        },
                        new IdentityRole
                        {
                            Name = RoleNames.ProjectManager,
                            NormalizedName = RoleNames.ProjectManager.ToUpperInvariant(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        },
                        new IdentityRole
                        {
                            Name = RoleNames.Developer,
                            NormalizedName = RoleNames.Developer.ToUpperInvariant(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        },
                        new IdentityRole
                        {
                            Name = RoleNames.Submitter,
                            NormalizedName = RoleNames.Submitter.ToUpperInvariant(),
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        }
                    ]);

                    await ctx.SaveChangesAsync(ct);
                });
        }

        return new AppDbContext(optionsBuilder.Options);
    }
}