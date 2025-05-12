using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Data.Models;

namespace Zap.Api.Data;

public static class DbContextOptionsExtensions
{
    public static void UseTicketTypeSeeding(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSeeding((ctx, _) =>
        {
            ctx.Set<TicketType>().AddRange([
                new TicketType(){
                    Name = TicketTypes.ChangeRequest
                },
                new TicketType(){
                    Name = TicketTypes.Defect
                },
                new TicketType(){
                    Name = TicketTypes.Enhanecment
                },
                new TicketType(){
                    Name = TicketTypes.Feature
                },
                new TicketType(){
                    Name = TicketTypes.GeneralTask
                },
                new TicketType(){
                    Name = TicketTypes.WorkTask
                },
            ]);
            ctx.Set<TicketPriority>().AddRange([
                new TicketPriority(){
                    Name = Priorites.Low
                },
                new TicketPriority(){
                    Name = Priorites.Medium
                },
                new TicketPriority(){
                    Name = Priorites.High
                },
                new TicketPriority(){
                    Name = Priorites.Urgent
                },
            ]);
            ctx.Set<TicketStatus>().AddRange([
                new TicketStatus(){
                    Name = TicketStatuses.InDevelopment
                },
                new TicketStatus(){
                    Name = TicketStatuses.New
                },
                new TicketStatus(){
                    Name = TicketStatuses.Resolved
                },
                new TicketStatus(){
                    Name = TicketStatuses.Testing
                },
            ]);

            ctx.SaveChanges();
        })
        .UseAsyncSeeding(async (ctx, _, ct) =>
        {
            await ctx.Set<TicketType>().AddRangeAsync([
                new TicketType(){
                    Name = TicketTypes.ChangeRequest
                },
                new TicketType(){
                    Name = TicketTypes.Defect
                },
                new TicketType(){
                    Name = TicketTypes.Enhanecment
                },
                new TicketType(){
                    Name = TicketTypes.Feature
                },
                new TicketType(){
                    Name = TicketTypes.GeneralTask
                },
                new TicketType(){
                    Name = TicketTypes.WorkTask
                },
            ]);
            await ctx.Set<TicketPriority>().AddRangeAsync([
                new TicketPriority(){
                    Name = Priorites.Low
                },
                new TicketPriority(){
                    Name = Priorites.Medium
                },
                new TicketPriority(){
                    Name = Priorites.High
                },
                new TicketPriority(){
                    Name = Priorites.Urgent
                },
            ]);
            await ctx.Set<TicketStatus>().AddRangeAsync([
                new TicketStatus(){
                    Name = TicketStatuses.InDevelopment
                },
                new TicketStatus(){
                    Name = TicketStatuses.New
                },
                new TicketStatus(){
                    Name = TicketStatuses.Resolved
                },
                new TicketStatus(){
                    Name = TicketStatuses.Testing
                },
            ]);
            await ctx.SaveChangesAsync();
        });
    }

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
