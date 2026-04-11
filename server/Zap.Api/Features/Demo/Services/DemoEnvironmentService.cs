using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Enums;
using Zap.Api.Common.Extensions;
using Zap.Api.Data;
using Zap.Api.Data.Models;

namespace Zap.Api.Features.Demo.Services;

public sealed class DemoEnvironmentService(
    AppDbContext db,
    UserManager<AppUser> userManager,
    ILogger<DemoEnvironmentService> logger) : IDemoEnvironmentService
{
    private const string DemoCompanyName = "Zap Demo Co";

    private const string DemoCompanyDescription =
        "A curated demonstration workspace that showcases each role in Zap with realistic projects and tickets.";

    private static readonly DemoUserSeed[] DemoUsers =
    [
        new(DemoRoleKeys.Admin, RoleNames.Admin, "demo-admin@zapdemo.local", "Demo", "Admin"),
        new(DemoRoleKeys.ProjectManager, RoleNames.ProjectManager, "demo-pm@zapdemo.local", "Demo", "Project Manager"),
        new(DemoRoleKeys.Developer, RoleNames.Developer, "demo-dev@zapdemo.local", "Demo", "Developer"),
        new(DemoRoleKeys.Submitter, RoleNames.Submitter, "demo-submitter@zapdemo.local", "Demo", "Submitter")
    ];

    public async Task EnsureDemoEnvironmentAsync()
    {
        if (await DemoEnvironmentIsCompleteAsync()) return;

        await ResetDemoEnvironmentAsync();
    }

    public async Task<AppUser?> GetDemoUserByRoleAsync(string roleKey)
    {
        if (!DemoRoleKeys.ToList().Contains(roleKey)) return null;

        await EnsureDemoEnvironmentAsync();

        var demoUserSeed = DemoUsers.FirstOrDefault(x => x.RoleKey == roleKey);
        if (demoUserSeed == null) return null;

        return await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IsDemo && u.Email == demoUserSeed.Email);
    }

    public async Task ResetDemoEnvironmentAsync()
    {
        logger.LogInformation("Resetting demo environment");

        await DeleteExistingDemoEnvironmentAsync();
        await CreateDemoEnvironmentAsync();

        logger.LogInformation("Demo environment reset completed");
    }

    private async Task<bool> DemoEnvironmentIsCompleteAsync()
    {
        var company = await db.Companies
            .AsNoTracking()
            .Where(c => c.IsDemo)
            .Select(c => new { c.Id, c.Name })
            .SingleOrDefaultAsync();

        if (company == null || company.Name != DemoCompanyName) return false;

        var demoMemberCount = await db.CompanyMembers
            .AsNoTracking()
            .CountAsync(m => m.CompanyId == company.Id && m.User != null && m.User.IsDemo);

        if (demoMemberCount != DemoUsers.Length) return false;

        var projectCount = await db.Projects.AsNoTracking().CountAsync(p => p.CompanyId == company.Id);
        var ticketCount = await db.Tickets.AsNoTracking().CountAsync(t => t.Project.CompanyId == company.Id);

        return projectCount >= 3 && ticketCount >= 6;
    }

    private async Task DeleteExistingDemoEnvironmentAsync()
    {
        var demoCompanyIds = await db.Companies
            .Where(c => c.IsDemo)
            .Select(c => c.Id)
            .ToListAsync();

        var demoUserIds = await db.Users
            .Where(u => u.IsDemo)
            .Select(u => u.Id)
            .ToListAsync();

        if (demoCompanyIds.Count == 0 && demoUserIds.Count == 0) return;

        var projectIds = await db.Projects
            .Where(p => demoCompanyIds.Contains(p.CompanyId))
            .Select(p => p.Id)
            .ToListAsync();

        var ticketIds = await db.Tickets
            .Where(t => projectIds.Contains(t.ProjectId))
            .Select(t => t.Id)
            .ToListAsync();

        if (ticketIds.Count != 0)
        {
            db.TicketComments.RemoveRange(await db.TicketComments.Where(c => ticketIds.Contains(c.TicketId))
                .ToListAsync());
            db.TicketAttachments.RemoveRange(await db.TicketAttachments.Where(a => ticketIds.Contains(a.TicketId))
                .ToListAsync());
            db.TicketHistories.RemoveRange(await db.TicketHistories.Where(h => ticketIds.Contains(h.TicketId))
                .ToListAsync());
            db.Tickets.RemoveRange(await db.Tickets.Where(t => ticketIds.Contains(t.Id)).ToListAsync());
        }

        if (projectIds.Count != 0)
        {
            var projects = await db.Projects
                .Include(p => p.AssignedMembers)
                .Where(p => projectIds.Contains(p.Id))
                .ToListAsync();

            foreach (var project in projects) project.AssignedMembers.Clear();

            db.Projects.RemoveRange(projects);
        }

        if (demoCompanyIds.Count != 0)
        {
            db.CompanyMembers.RemoveRange(await db.CompanyMembers.Where(m => demoCompanyIds.Contains(m.CompanyId!))
                .ToListAsync());
            db.Companies.RemoveRange(await db.Companies.Where(c => demoCompanyIds.Contains(c.Id)).ToListAsync());
        }

        await db.SaveChangesAsync();

        if (demoUserIds.Count == 0) return;

        var demoUsers = await db.Users.Where(u => demoUserIds.Contains(u.Id)).ToListAsync();
        foreach (var user in demoUsers)
        {
            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to delete demo user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    private async Task CreateDemoEnvironmentAsync()
    {
        var roles = await db.CompanyRoles.ToDictionaryAsync(r => r.Name);
        var usersByRole = new Dictionary<string, AppUser>();

        foreach (var demoUser in DemoUsers)
        {
            var user = new AppUser
            {
                Email = demoUser.Email,
                UserName = demoUser.Email,
                FirstName = demoUser.FirstName,
                LastName = demoUser.LastName,
                EmailConfirmed = true,
                IsDemo = true
            };
            user.SetDefaultAvatar();

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create demo user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddCustomClaimsAsync(user);
            usersByRole[demoUser.RoleName] = user;
        }

        var company = new Company
        {
            Name = DemoCompanyName,
            Description = DemoCompanyDescription,
            WebsiteUrl = "https://demo.zap.local",
            OwnerId = usersByRole[RoleNames.Admin].Id,
            IsDemo = true
        };

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var membersByRole = new Dictionary<string, CompanyMember>();
        foreach (var demoUser in DemoUsers)
        {
            var member = new CompanyMember
            {
                UserId = usersByRole[demoUser.RoleName].Id,
                CompanyId = company.Id,
                RoleId = roles[demoUser.RoleName].Id
            };

            db.CompanyMembers.Add(member);
            membersByRole[demoUser.RoleName] = member;
        }

        await db.SaveChangesAsync();

        var triageProject = new Project
        {
            Name = "Platform Triage",
            Description = "Core bug triage and customer-impact issues that show the full ticket workflow.",
            Priority = Priorities.High,
            DueDate = DateTime.UtcNow.AddDays(21),
            CompanyId = company.Id,
            ProjectManagerId = membersByRole[RoleNames.ProjectManager].Id,
            AssignedMembers =
            [
                membersByRole[RoleNames.ProjectManager],
                membersByRole[RoleNames.Developer],
                membersByRole[RoleNames.Submitter]
            ]
        };

        var mobileProject = new Project
        {
            Name = "Mobile Experience",
            Description = "A feature-focused project with active development and backlog work for product demos.",
            Priority = Priorities.Medium,
            DueDate = DateTime.UtcNow.AddDays(45),
            CompanyId = company.Id,
            ProjectManagerId = membersByRole[RoleNames.ProjectManager].Id,
            AssignedMembers =
            [
                membersByRole[RoleNames.ProjectManager],
                membersByRole[RoleNames.Developer],
                membersByRole[RoleNames.Submitter]
            ]
        };

        var operationsProject = new Project
        {
            Name = "Operations Reporting",
            Description = "An admin-facing project that highlights archived and completed work across teams.",
            Priority = Priorities.Low,
            DueDate = DateTime.UtcNow.AddDays(60),
            CompanyId = company.Id,
            ProjectManagerId = membersByRole[RoleNames.ProjectManager].Id,
            AssignedMembers =
            [
                membersByRole[RoleNames.ProjectManager],
                membersByRole[RoleNames.Developer],
                membersByRole[RoleNames.Submitter]
            ]
        };

        db.Projects.AddRange(triageProject, mobileProject, operationsProject);
        await db.SaveChangesAsync();

        var statuses = await db.TicketStatuses.ToDictionaryAsync(x => x.Name);
        var priorities = await db.TicketPriorities.ToDictionaryAsync(x => x.Name);
        var types = await db.TicketTypes.ToDictionaryAsync(x => x.Name);

        var tickets = new List<Ticket>
        {
            CreateTicket(
                triageProject,
                "Investigate login failures for invited users",
                "Some invited users reach the app but fail to complete sign-in after the callback returns.",
                priorities[Priorities.Urgent].Id,
                statuses[TicketStatuses.InDevelopment].Id,
                types[TicketTypes.Defect].Id,
                membersByRole[RoleNames.Submitter].Id,
                membersByRole[RoleNames.Developer].Id),
            CreateTicket(
                triageProject,
                "Review tenant authorization audit trail",
                "Confirm the recent authorization updates still log meaningful history for company-scoped actions.",
                priorities[Priorities.High].Id,
                statuses[TicketStatuses.Testing].Id,
                types[TicketTypes.WorkTask].Id,
                membersByRole[RoleNames.Admin].Id,
                membersByRole[RoleNames.Developer].Id),
            CreateTicket(
                mobileProject,
                "Polish mobile ticket detail layout",
                "The ticket detail page needs spacing and readability improvements on smaller screens.",
                priorities[Priorities.Medium].Id,
                statuses[TicketStatuses.New].Id,
                types[TicketTypes.Enhanecment].Id,
                membersByRole[RoleNames.Submitter].Id,
                null),
            CreateTicket(
                mobileProject,
                "Add offline-ready comment drafts",
                "Draft comment support should persist locally when the network connection drops.",
                priorities[Priorities.Medium].Id,
                statuses[TicketStatuses.InDevelopment].Id,
                types[TicketTypes.Feature].Id,
                membersByRole[RoleNames.ProjectManager].Id,
                membersByRole[RoleNames.Developer].Id),
            CreateTicket(
                operationsProject,
                "Export company activity report to CSV",
                "Operations wants a lightweight export for weekly leadership updates.",
                priorities[Priorities.Low].Id,
                statuses[TicketStatuses.Resolved].Id,
                types[TicketTypes.ChangeRequest].Id,
                membersByRole[RoleNames.Admin].Id,
                membersByRole[RoleNames.Developer].Id),
            CreateTicket(
                operationsProject,
                "Archive stale internal dashboard widgets",
                "Remove outdated widgets from the operations dashboard and keep the configuration history clear.",
                priorities[Priorities.Low].Id,
                statuses[TicketStatuses.Testing].Id,
                types[TicketTypes.GeneralTask].Id,
                membersByRole[RoleNames.ProjectManager].Id,
                membersByRole[RoleNames.Developer].Id)
        };

        db.Tickets.AddRange(tickets);
        await db.SaveChangesAsync();

        var comments = new List<TicketComment>
        {
            new()
            {
                TicketId = tickets[0].Id,
                SenderId = membersByRole[RoleNames.Submitter].Id,
                Message = "Customer support confirmed the issue happens most often after a refresh token rotation."
            },
            new()
            {
                TicketId = tickets[0].Id,
                SenderId = membersByRole[RoleNames.Developer].Id,
                Message = "I can reproduce it locally. I am checking the callback and session refresh flow now."
            },
            new()
            {
                TicketId = tickets[3].Id,
                SenderId = membersByRole[RoleNames.ProjectManager].Id,
                Message = "This is a good candidate feature for the next customer demo once drafts sync correctly."
            }
        };

        var historyEntries = new List<TicketHistory>
        {
            new()
            {
                TicketId = tickets[0].Id,
                CreatorId = membersByRole[RoleNames.ProjectManager].Id,
                OldValue = TicketStatuses.New,
                NewValue = TicketStatuses.InDevelopment,
                RelatedEntityName = nameof(TicketStatus),
                RelatedEntityId = statuses[TicketStatuses.InDevelopment].Id,
                Type = TicketHistoryTypes.UpdateStatus
            },
            new()
            {
                TicketId = tickets[4].Id,
                CreatorId = membersByRole[RoleNames.Developer].Id,
                OldValue = TicketStatuses.InDevelopment,
                NewValue = TicketStatuses.Resolved,
                RelatedEntityName = nameof(TicketStatus),
                RelatedEntityId = statuses[TicketStatuses.Resolved].Id,
                Type = TicketHistoryTypes.Resolved
            }
        };

        db.TicketComments.AddRange(comments);
        db.TicketHistories.AddRange(historyEntries);
        await db.SaveChangesAsync();
    }

    private static Ticket CreateTicket(Project project, string name, string description, string priorityId,
        string statusId,
        string typeId, string submitterId, string? assigneeId)
    {
        return new Ticket
        {
            Name = name,
            Description = description,
            ProjectId = project.Id,
            PriorityId = priorityId,
            StatusId = statusId,
            TypeId = typeId,
            SubmitterId = submitterId,
            AssigneeId = assigneeId
        };
    }

    private sealed record DemoUserSeed(
        string RoleKey,
        string RoleName,
        string Email,
        string FirstName,
        string LastName);
}