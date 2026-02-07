using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Data.Models;

namespace Zap.Api.Data;

public static class DbContextOptionsExtensions
{
    public static void UseRequiredSeeding(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSeeding((ctx, _) =>
            {
                ctx.SetTicketTypes();
                ctx.SetRoles();
                ctx.SaveChanges();
            })
            .UseAsyncSeeding(async (ctx, _, _) =>
            {
                await ctx.SetTicketTypesAsync();
                await ctx.SetRolesAsync();
                await ctx.SaveChangesAsync();
            });
    }

    private static void SetTicketTypes(this DbContext ctx)
    {
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
    }

    private static async Task SetTicketTypesAsync(this DbContext ctx)
    {
        await ctx.Set<TicketType>().AddRangeAsync(new TicketType { Name = TicketTypes.ChangeRequest },
            new TicketType { Name = TicketTypes.Defect }, new TicketType { Name = TicketTypes.Enhanecment },
            new TicketType { Name = TicketTypes.Feature }, new TicketType { Name = TicketTypes.GeneralTask },
            new TicketType { Name = TicketTypes.WorkTask });
        await ctx.Set<TicketPriority>().AddRangeAsync(new TicketPriority { Name = Priorities.Low },
            new TicketPriority { Name = Priorities.Medium }, new TicketPriority { Name = Priorities.High },
            new TicketPriority { Name = Priorities.Urgent });
        await ctx.Set<TicketStatus>().AddRangeAsync(new TicketStatus { Name = TicketStatuses.InDevelopment },
            new TicketStatus { Name = TicketStatuses.New }, new TicketStatus { Name = TicketStatuses.Resolved },
            new TicketStatus { Name = TicketStatuses.Testing });
    }

    private static void SetRoles(this DbContext ctx)
    {
        var role = ctx.Set<CompanyRole>().FirstOrDefault(x => x.Name == RoleNames.Admin);
        if (role != null) return;

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
    }

    private static async Task SetRolesAsync(this DbContext ctx)
    {
        var role = await ctx.Set<CompanyRole>()
            .FirstOrDefaultAsync(x => x.Name == RoleNames.Admin);
        if (role != null) return;

        await ctx.Set<CompanyRole>().AddRangeAsync(new CompanyRole
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
    }
}