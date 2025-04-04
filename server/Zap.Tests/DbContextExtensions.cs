using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zap.Tests;

public static class DbContextExtensions
{
    public static IServiceCollection AddDbContextOptions(this IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<AppDbContext>>();

        var ob = new DbContextOptionsBuilder<AppDbContext>();
        ob.UseInMemoryDatabase("Zap.Tests");
        ob.UseSeeding((ctx, _) =>
        {
            // Do this check because this runs each test for some reason
            var res = ctx.Roles.FirstOrDefault(x => x.Name == RoleNames.Admin);
            if (res != null) return;

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
        });

        services.AddSingleton(ob.Options);
        // The untyped version just calls the typed one
        services.AddSingleton<DbContextOptions>(s => s.GetRequiredService<DbContextOptions<AppDbContext>>());
        
        return services;
        ;
    }
}