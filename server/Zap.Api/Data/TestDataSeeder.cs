using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;
using Zap.Api.Common.Extensions;
using Zap.Api.Data.Models;

namespace Zap.Api.Data;

public static class TestDataSeeder
{
    private const string TestUserEmail = "test@test.com";
    private const string TestUserPassword = "Password1!";

    public static async Task SeedTestDataAsync(
        IServiceProvider services,
        ILogger logger,
        IWebHostEnvironment env)
    {
        // Only seed in Development environment
        if (!env.IsDevelopment())
        {
            logger.LogInformation("Skipping test data seeding - not in Development environment.");
            return;
        }

        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        // Check if test user already exists - if so, skip all seeding
        var existingUser = await userManager.FindByEmailAsync(TestUserEmail);
        if (existingUser != null)
        {
            logger.LogInformation("Test user already exists - skipping test data seeding.");
            return;
        }

        logger.LogInformation("Beginning test data seeding...");

        // Seed test user and company
        var (testUser, testCompany, testMember) = await SeedTestUserAndCompanyAsync(userManager, db, logger);

        // Seed additional company members
        var additionalMembers = await SeedAdditionalMembersAsync(userManager, db, testCompany, logger);

        // Combine all members for project assignment
        var allMembers = new List<CompanyMember> { testMember };
        allMembers.AddRange(additionalMembers);

        // Seed projects
        var projects = await SeedProjectsAsync(db, testCompany, allMembers, logger);

        // Seed tickets
        await SeedTicketsAsync(db, projects, allMembers, logger);

        logger.LogInformation("Test data seeding completed successfully.");
    }

    private static async Task<(AppUser user, Company company, CompanyMember member)> SeedTestUserAndCompanyAsync(
        UserManager<AppUser> userManager,
        AppDbContext db,
        ILogger logger)
    {
        // Create test user
        var testUser = new AppUser
        {
            Email = TestUserEmail,
            UserName = TestUserEmail,
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true
        };
        testUser.SetDefaultAvatar();

        var result = await userManager.CreateAsync(testUser, TestUserPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create test user: {errors}");
        }

        await userManager.AddCustomClaimsAsync(testUser);
        logger.LogInformation("Created test user: {Email}", TestUserEmail);

        // Create test company
        var testCompany = new Company
        {
            Name = "Test Company",
            Description = "This is a test company with sample data for development purposes.",
            OwnerId = testUser.Id,
            WebsiteUrl = "https://example.com"
        };

        db.Companies.Add(testCompany);
        await db.SaveChangesAsync();
        logger.LogInformation("Created test company: {CompanyName}", testCompany.Name);

        // Get Admin role
        var adminRole = await db.CompanyRoles.FirstOrDefaultAsync(r => r.Name == RoleNames.Admin);
        if (adminRole == null)
        {
            throw new InvalidOperationException("Admin role not found. Ensure database seeding has run.");
        }

        // Create company member for test user
        var testMember = new CompanyMember
        {
            UserId = testUser.Id,
            CompanyId = testCompany.Id,
            RoleId = adminRole.Id
        };

        db.CompanyMembers.Add(testMember);
        await db.SaveChangesAsync();
        logger.LogInformation("Added test user as company member with Admin role.");

        return (testUser, testCompany, testMember);
    }

    private static async Task<List<CompanyMember>> SeedAdditionalMembersAsync(
        UserManager<AppUser> userManager,
        AppDbContext db,
        Company company,
        ILogger logger)
    {
        var roles = await db.CompanyRoles.ToListAsync();
        var members = new List<CompanyMember>();

        // Create 8 additional fake users with varied roles
        var fakeUsers = new Faker<AppUser>()
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .RuleFor(u => u.EmailConfirmed, _ => true)
            .Generate(8);

        // Assign varied roles: 2 Project Managers, 4 Developers, 2 Submitters
        var roleAssignments = new[]
        {
            RoleNames.ProjectManager,
            RoleNames.ProjectManager,
            RoleNames.Developer,
            RoleNames.Developer,
            RoleNames.Developer,
            RoleNames.Developer,
            RoleNames.Submitter,
            RoleNames.Submitter
        };

        for (int i = 0; i < fakeUsers.Count; i++)
        {
            var user = fakeUsers[i];
            user.AvatarUrl = $"https://ui-avatars.com/api/?name={user.FirstName}+{user.LastName}";

            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                var role = roles.FirstOrDefault(r => r.Name == roleAssignments[i]);
                if (role != null)
                {
                    var member = new CompanyMember
                    {
                        UserId = user.Id,
                        CompanyId = company.Id,
                        RoleId = role.Id
                    };

                    db.CompanyMembers.Add(member);
                    members.Add(member);
                }
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Created {Count} additional company members.", members.Count);

        return members;
    }

    private static async Task<List<Project>> SeedProjectsAsync(
        AppDbContext db,
        Company company,
        List<CompanyMember> allMembers,
        ILogger logger)
    {
        var projects = new List<Project>();

        // Get project managers from the members list
        var projectManagers = allMembers
            .Where(m => m.Role?.Name == RoleNames.ProjectManager)
            .ToList();

        // Project 1: Active project with urgent priority
        var project1 = new Project
        {
            Name = "Customer Portal Redesign",
            Description = "Complete overhaul of the customer-facing portal with modern UI/UX design and improved performance.",
            Priority = Priorities.High,
            DueDate = DateTime.UtcNow.AddMonths(2),
            CompanyId = company.Id,
            ProjectManagerId = projectManagers.FirstOrDefault()?.Id,
            IsArchived = false
        };

        // Assign 4-5 members to project 1
        var project1Members = allMembers
            .Where(m => m.Role?.Name == RoleNames.Developer || m.Role?.Name == RoleNames.Submitter)
            .Take(5)
            .ToList();
        project1.AssignedMembers = project1Members;

        projects.Add(project1);

        // Project 2: Medium priority project
        var project2 = new Project
        {
            Name = "Mobile App Development",
            Description = "Native mobile applications for iOS and Android platforms with offline capabilities.",
            Priority = Priorities.Medium,
            DueDate = DateTime.UtcNow.AddMonths(4),
            CompanyId = company.Id,
            ProjectManagerId = projectManagers.Skip(1).FirstOrDefault()?.Id,
            IsArchived = false
        };

        // Assign 3-4 members to project 2
        var project2Members = allMembers
            .Where(m => m.Role?.Name == RoleNames.Developer)
            .Skip(2)
            .Take(4)
            .ToList();
        project2.AssignedMembers = project2Members;

        projects.Add(project2);

        // Project 3: Low priority / maintenance project
        var project3 = new Project
        {
            Name = "API Documentation & Maintenance",
            Description = "Ongoing maintenance and documentation improvements for the REST API services.",
            Priority = Priorities.Low,
            DueDate = DateTime.UtcNow.AddMonths(6),
            CompanyId = company.Id,
            ProjectManagerId = projectManagers.FirstOrDefault()?.Id,
            IsArchived = false
        };

        // Assign 2-3 members to project 3
        var project3Members = allMembers
            .Where(m => m.Role?.Name == RoleNames.Developer || m.Role?.Name == RoleNames.Submitter)
            .Skip(1)
            .Take(3)
            .ToList();
        project3.AssignedMembers = project3Members;

        projects.Add(project3);

        db.Projects.AddRange(projects);
        await db.SaveChangesAsync();

        logger.LogInformation("Created {Count} test projects.", projects.Count);

        return projects;
    }

    private static async Task SeedTicketsAsync(
        AppDbContext db,
        List<Project> projects,
        List<CompanyMember> allMembers,
        ILogger logger)
    {
        // Load ticket metadata
        var ticketTypes = await db.TicketTypes.ToListAsync();
        var ticketPriorities = await db.TicketPriorities.ToListAsync();
        var ticketStatuses = await db.TicketStatuses.ToListAsync();

        var tickets = new List<Ticket>();

        // Helper to get random items
        var random = new Random();
        T GetRandom<T>(List<T> items) => items[random.Next(items.Count)];

        // Create 15 tickets distributed across projects
        var ticketTemplates = new[]
        {
            new { Name = "Fix login authentication issue", Description = "Users are unable to log in with valid credentials intermittently.", Type = TicketTypes.Defect, Priority = Priorities.Urgent, Status = TicketStatuses.InDevelopment },
            new { Name = "Implement dark mode", Description = "Add dark mode theme support across the entire application.", Type = TicketTypes.Feature, Priority = Priorities.Medium, Status = TicketStatuses.New },
            new { Name = "Optimize database queries", Description = "Improve query performance for dashboard loading.", Type = TicketTypes.Enhanecment, Priority = Priorities.High, Status = TicketStatuses.InDevelopment },
            new { Name = "Add export to CSV functionality", Description = "Users should be able to export reports to CSV format.", Type = TicketTypes.Feature, Priority = Priorities.Low, Status = TicketStatuses.New },
            new { Name = "Fix broken images on profile page", Description = "Profile images are not displaying correctly for some users.", Type = TicketTypes.Defect, Priority = Priorities.Medium, Status = TicketStatuses.Testing },
            new { Name = "Update API documentation", Description = "Refresh API documentation with new endpoints and examples.", Type = TicketTypes.GeneralTask, Priority = Priorities.Low, Status = TicketStatuses.InDevelopment },
            new { Name = "Mobile responsive layout fixes", Description = "Several pages are not displaying correctly on mobile devices.", Type = TicketTypes.Defect, Priority = Priorities.High, Status = TicketStatuses.New },
            new { Name = "Add password strength indicator", Description = "Display password strength when users create or change passwords.", Type = TicketTypes.Enhanecment, Priority = Priorities.Low, Status = TicketStatuses.Resolved },
            new { Name = "Implement two-factor authentication", Description = "Add 2FA support for enhanced security.", Type = TicketTypes.Feature, Priority = Priorities.High, Status = TicketStatuses.InDevelopment },
            new { Name = "Fix memory leak in dashboard", Description = "Dashboard component has a memory leak causing performance degradation.", Type = TicketTypes.Defect, Priority = Priorities.Urgent, Status = TicketStatuses.Testing },
            new { Name = "Add user activity logging", Description = "Track and log user activities for audit purposes.", Type = TicketTypes.Feature, Priority = Priorities.Medium, Status = TicketStatuses.New },
            new { Name = "Refactor authentication module", Description = "Clean up and refactor authentication code for better maintainability.", Type = TicketTypes.WorkTask, Priority = Priorities.Low, Status = TicketStatuses.Resolved },
            new { Name = "Update dependencies to latest versions", Description = "Update all npm and NuGet packages to latest stable versions.", Type = TicketTypes.ChangeRequest, Priority = Priorities.Medium, Status = TicketStatuses.InDevelopment },
            new { Name = "Add search filters to ticket list", Description = "Users need advanced filtering options on the ticket list page.", Type = TicketTypes.Enhanecment, Priority = Priorities.Medium, Status = TicketStatuses.New },
            new { Name = "Fix timezone handling", Description = "Dates are displaying in incorrect timezone for some users.", Type = TicketTypes.Defect, Priority = Priorities.High, Status = TicketStatuses.Testing }
        };

        // Distribute tickets across projects
        for (int i = 0; i < ticketTemplates.Length; i++)
        {
            var template = ticketTemplates[i];
            var project = projects[i % projects.Count]; // Round-robin distribution

            // Get members assigned to this project
            var projectMembers = project.AssignedMembers.ToList();
            if (!projectMembers.Any()) continue;

            var submitter = GetRandom(projectMembers);
            var assignee = random.Next(100) > 30 ? GetRandom(projectMembers) : null; // 70% assigned

            var ticketType = ticketTypes.FirstOrDefault(t => t.Name == template.Type);
            var ticketPriority = ticketPriorities.FirstOrDefault(p => p.Name == template.Priority);
            var ticketStatus = ticketStatuses.FirstOrDefault(s => s.Name == template.Status);

            if (ticketType == null || ticketPriority == null || ticketStatus == null) continue;

            var ticket = new Ticket
            {
                Name = template.Name,
                Description = template.Description,
                ProjectId = project.Id,
                TypeId = ticketType.Id,
                PriorityId = ticketPriority.Id,
                StatusId = ticketStatus.Id,
                SubmitterId = submitter.Id,
                AssigneeId = assignee?.Id,
                IsArchived = false
            };

            tickets.Add(ticket);
        }

        db.Tickets.AddRange(tickets);
        await db.SaveChangesAsync();

        logger.LogInformation("Created {Count} test tickets.", tickets.Count);
    }
}
