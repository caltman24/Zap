using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Data.Models;

namespace Zap.Api.Data;

public static class DbContextOptionsExtensions
{
    // TODO: Seed Roles into different table
    public static void UseRoleSeeding(this DbContextOptionsBuilder optionsBuilder)
    {
        // Called as part of EnsureCreated, Migrate, and `dotnet ef database update`
        optionsBuilder.UseSeeding((ctx, _) =>
            {
                var role = ctx.Set<CompanyRole>().FirstOrDefault(x => x.Name == RoleNames.Admin);
                if (role != null) return;

                ctx.Set<CompanyRole>().AddRange([
                    new CompanyRole
                    {
                        Name = RoleNames.Admin,
                        NormalizedName = RoleNames.Admin.ToUpperInvariant(),
                    },
                    new CompanyRole
                    {
                        Name = RoleNames.ProjectManager,
                        NormalizedName = RoleNames.ProjectManager.ToUpperInvariant(),
                    },
                    new CompanyRole
                    {
                        Name = RoleNames.Developer,
                        NormalizedName = RoleNames.Developer.ToUpperInvariant(),
                    },
                    new CompanyRole
                    {
                        Name = RoleNames.Submitter,
                        NormalizedName = RoleNames.Submitter.ToUpperInvariant(),
                    }
                ]);

                ctx.SaveChanges();
            })
            .UseAsyncSeeding(async (ctx, _, ct) =>
            {
                var role = await ctx.Set<CompanyRole>()
                    .FirstOrDefaultAsync(x => x.Name == RoleNames.Admin, cancellationToken: ct);
                if (role != null) return;

                await ctx.Set<CompanyRole>().AddRangeAsync([
                    new CompanyRole
                    {
                        Name = RoleNames.Admin,
                        NormalizedName = RoleNames.Admin.ToUpperInvariant(),
                    },
                    new CompanyRole
                    {
                        Name = RoleNames.ProjectManager,
                        NormalizedName = RoleNames.ProjectManager.ToUpperInvariant(),
                    },
                    new CompanyRole
                    {
                        Name = RoleNames.Developer,
                        NormalizedName = RoleNames.Developer.ToUpperInvariant(),
                    },
                    new CompanyRole
                    {
                        Name = RoleNames.Submitter,
                        NormalizedName = RoleNames.Submitter.ToUpperInvariant(),
                    }
                ]);

                await ctx.SaveChangesAsync(ct);
            });
    }
}
