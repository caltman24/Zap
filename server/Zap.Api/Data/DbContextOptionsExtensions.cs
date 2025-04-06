using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;

namespace Zap.Api.Data;

public static class DbContextOptionsExtensions
{
    public static void UseRoleSeeding(this DbContextOptionsBuilder optionsBuilder)
    {
        // Called as part of EnsureCreated, Migrate, and `dotnet ef database update`
        optionsBuilder.UseSeeding((ctx, _) =>
            {
                var role = ctx.Set<IdentityRole>().FirstOrDefault(x => x.Name == RoleNames.Admin);
                if (role != null) return;

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
                var role = await ctx.Set<IdentityRole>()
                    .FirstOrDefaultAsync(x => x.Name == RoleNames.Admin, cancellationToken: ct);
                if (role != null) return;

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
}