using Microsoft.Extensions.DependencyInjection.Extensions;

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

            ctx.Set<CompanyRole>().AddRange(new CompanyRole
            {
                Name = RoleNames.Admin,
                NormalizedName = RoleNames.Admin.ToUpperInvariant()
            }, new CompanyRole
            {
                Name = RoleNames.ProjectManager,
                NormalizedName = RoleNames.ProjectManager.ToUpperInvariant()
            }, new CompanyRole
            {
                Name = RoleNames.Developer,
                NormalizedName = RoleNames.Developer.ToUpperInvariant()
            }, new CompanyRole
            {
                Name = RoleNames.Submitter,
                NormalizedName = RoleNames.Submitter.ToUpperInvariant()
            });

            // Seed ticket lookup tables (matching production UseRequiredSeeding)
            ctx.Set<TicketType>().AddRange(new TicketType { Name = TicketTypes.ChangeRequest },
                new TicketType { Name = TicketTypes.Defect }, new TicketType { Name = TicketTypes.Enhanecment },
                new TicketType { Name = TicketTypes.Feature }, new TicketType { Name = TicketTypes.GeneralTask },
                new TicketType { Name = TicketTypes.WorkTask });

            ctx.Set<TicketPriority>().AddRange(new TicketPriority { Name = Priorities.Low },
                new TicketPriority { Name = Priorities.Medium }, new TicketPriority { Name = Priorities.High },
                new TicketPriority { Name = Priorities.Urgent });

            ctx.Set<TicketStatus>().AddRange(new TicketStatus { Name = TicketStatuses.InDevelopment },
                new TicketStatus { Name = TicketStatuses.New }, new TicketStatus { Name = TicketStatuses.Resolved },
                new TicketStatus { Name = TicketStatuses.Testing });

            ctx.SaveChanges();
        });

        services.AddSingleton(ob.Options);
        // The untyped version just calls the typed one
        services.AddSingleton<DbContextOptions>(s => s.GetRequiredService<DbContextOptions<AppDbContext>>());
        return services;
    }
}