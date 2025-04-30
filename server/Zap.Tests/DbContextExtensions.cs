using Microsoft.Extensions.DependencyInjection.Extensions;
using Zap.Api.Common.Constants;
using Zap.Api.Data;

namespace Zap.Tests;

public static class DbContextExtensions
{
    public static IServiceCollection AddDbContextOptions(this IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<AppDbContext>>();

        var ob = new DbContextOptionsBuilder<AppDbContext>();
        ob.UseInMemoryDatabase($"ZapTests_{Guid.NewGuid().ToString()}");
        ob.UseSeeding((ctx, _) =>
        {
            // Do this check because this runs each test for some reason
            var res = ctx.CompanyRoles.FirstOrDefault(x => x.Name == RoleNames.Admin);
            if (res != null) return;

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
        });

        services.AddSingleton(ob.Options);
        // The untyped version just calls the typed one
        services.AddSingleton<DbContextOptions>(s => s.GetRequiredService<DbContextOptions<AppDbContext>>());
        return services;
        ;
    }
}
