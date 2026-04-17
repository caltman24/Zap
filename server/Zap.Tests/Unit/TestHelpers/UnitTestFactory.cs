using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Zap.Api.Features.FileUpload.Services;

namespace Zap.Tests.Unit.TestHelpers;

internal static class UnitTestFactory
{
    internal static CurrentUser CreateCurrentUser(
        string? roleName,
        string memberId = "member-1",
        string companyId = "company-1",
        string userId = "user-1")
    {
        var currentUser = new CurrentUser
        {
            User = new AppUser
            {
                Id = userId,
                Email = $"{userId}@test.com",
                UserName = $"{userId}@test.com",
                FirstName = "John",
                LastName = "Doe"
            }
        };

        if (roleName != null)
            currentUser.Member = new CompanyMember
            {
                Id = memberId,
                UserId = userId,
                CompanyId = companyId,
                RoleId = $"role-{roleName}",
                Role = new CompanyRole { Id = $"role-{roleName}", Name = roleName, NormalizedName = roleName.ToUpperInvariant() },
                User = currentUser.User
            };

        return currentUser;
    }

    internal static TicketHistory CreateHistoryEntry(
        TicketHistoryTypes type,
        string creatorName = "John Doe",
        string? oldValue = null,
        string? newValue = null,
        string? relatedEntityName = null)
    {
        var names = creatorName.Split(' ', 2, StringSplitOptions.TrimEntries);
        var firstName = names[0];
        var lastName = names.Length > 1 ? names[1] : "Doe";

        return new TicketHistory
        {
            TicketId = "ticket-1",
            CreatorId = "creator-1",
            Creator = new CompanyMember
            {
                Id = "creator-1",
                UserId = Guid.NewGuid().ToString(),
                RoleId = "role-1",
                User = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "formatter@test.com",
                    UserName = "formatter@test.com",
                    FirstName = firstName,
                    LastName = lastName
                },
                Role = new CompanyRole { Id = "role-1", Name = RoleNames.Admin, NormalizedName = RoleNames.Admin.ToUpperInvariant() }
            },
            Type = type,
            OldValue = oldValue,
            NewValue = newValue,
            RelatedEntityName = relatedEntityName,
            CreatedAt = new DateTime(2026, 4, 11, 10, 30, 0, DateTimeKind.Utc)
        };
    }

    internal static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    internal static BasicTicketDto CreateBasicTicket(
        string roleOwnerId,
        string? assigneeId = null,
        string? projectManagerId = null,
        string status = TicketStatuses.New,
        bool isArchived = false,
        string submitterId = "submitter-1")
    {
        return new BasicTicketDto(
            "ticket-1",
            "Ticket",
            "Description",
            Priorities.Low,
            status,
            TicketTypes.Defect,
            "project-1",
            projectManagerId,
            isArchived,
            false,
            DateTime.UtcNow,
            null,
            new MemberInfoDto(submitterId, "Submitter User", "avatar", RoleNames.Submitter),
            assigneeId == null ? null : new MemberInfoDto(assigneeId, "Assigned Dev", "avatar", RoleNames.Developer)
        );
    }

    internal static ProjectDto CreateProject(
        string? projectManagerId,
        IEnumerable<MemberInfoDto>? members = null,
        bool isArchived = false)
    {
        var memberList = members?.ToList() ?? [];

        return new ProjectDto(
            "project-1",
            "Project",
            "Description",
            "High",
            "company-1",
            projectManagerId == null ? null : new MemberInfoDto(projectManagerId, "Project Manager", "avatar", RoleNames.ProjectManager),
            isArchived,
            DateTime.UtcNow.AddDays(10),
            [],
            memberList);
    }

    internal static async Task<AuthorizationScenario> CreateAuthorizationScenarioAsync(AppDbContext db)
    {
        var adminRole = CreateRole(RoleNames.Admin);
        var pmRole = CreateRole(RoleNames.ProjectManager);
        var developerRole = CreateRole(RoleNames.Developer);
        var submitterRole = CreateRole(RoleNames.Submitter);

        db.CompanyRoles.AddRange(adminRole, pmRole, developerRole, submitterRole);

        var company = new Company
        {
            Id = "company-1",
            Name = "Company",
            Description = "Description"
        };

        var otherCompany = new Company
        {
            Id = "company-2",
            Name = "Other Company",
            Description = "Other Description"
        };

        db.Companies.AddRange(company, otherCompany);

        var admin = CreateMember("admin-1", "admin-user-1", company.Id, adminRole, "Admin", "User");
        var pm = CreateMember("pm-1", "pm-user-1", company.Id, pmRole, "Project", "Manager");
        var developer = CreateMember("dev-1", "dev-user-1", company.Id, developerRole, "Dev", "User");
        var submitter = CreateMember("submitter-1", "submitter-user-1", company.Id, submitterRole, "Submitter", "User");
        var otherDeveloper = CreateMember("dev-2", "dev-user-2", company.Id, developerRole, "Other", "Dev");
        var externalDeveloper = CreateMember("external-dev-1", "external-dev-user-1", otherCompany.Id, developerRole,
            "External", "Dev");

        company.Members.Add(admin);
        company.Members.Add(pm);
        company.Members.Add(developer);
        company.Members.Add(submitter);
        company.Members.Add(otherDeveloper);
        otherCompany.Members.Add(externalDeveloper);

        db.Users.AddRange(admin.User, pm.User, developer.User, submitter.User, otherDeveloper.User, externalDeveloper.User);
        db.CompanyMembers.AddRange(admin, pm, developer, submitter, otherDeveloper, externalDeveloper);

        var statusNew = new TicketStatus { Id = "status-new", Name = TicketStatuses.New };
        var priorityLow = new TicketPriority { Id = "priority-low", Name = Priorities.Low };
        var typeDefect = new TicketType { Id = "type-defect", Name = TicketTypes.Defect };
        db.TicketStatuses.Add(statusNew);
        db.TicketPriorities.Add(priorityLow);
        db.TicketTypes.Add(typeDefect);

        var project = new Project
        {
            Id = "project-1",
            Name = "Project",
            Description = "Project Description",
            Priority = "High",
            DueDate = DateTime.UtcNow.AddDays(10),
            CompanyId = company.Id,
            Company = company,
            ProjectManagerId = pm.Id,
            ProjectManager = pm
        };

        project.AssignedMembers.Add(developer);
        project.AssignedMembers.Add(submitter);
        developer.AssignedProjects.Add(project);
        submitter.AssignedProjects.Add(project);

        var otherProject = new Project
        {
            Id = "project-2",
            Name = "Other Project",
            Description = "Other Project Description",
            Priority = "Medium",
            DueDate = DateTime.UtcNow.AddDays(7),
            CompanyId = company.Id,
            Company = company
        };

        db.Projects.AddRange(project, otherProject);

        var ticket = new Ticket
        {
            Id = "ticket-1",
            DisplayId = "#ZAP-0001",
            Name = "Ticket",
            Description = "Ticket Description",
            ProjectId = project.Id,
            Project = project,
            PriorityId = priorityLow.Id,
            Priority = priorityLow,
            StatusId = statusNew.Id,
            Status = statusNew,
            TypeId = typeDefect.Id,
            Type = typeDefect,
            SubmitterId = submitter.Id,
            Submitter = submitter,
            AssigneeId = developer.Id,
            Assignee = developer
        };

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        return new AuthorizationScenario(company, otherCompany, admin, pm, developer, submitter, otherDeveloper,
            externalDeveloper, project, otherProject, ticket);
    }

    internal static TicketCommentsService CreateTicketCommentsService(AppDbContext db)
    {
        return new TicketCommentsService(db);
    }

    internal static CompanyService CreateCompanyService(AppDbContext db)
    {
        return new CompanyService(db, new StubFileUploadService(), NullLogger<CompanyService>.Instance);
    }

    internal static CompanyMember CreateMember(string memberId, string userId, string companyId, CompanyRole role,
        string firstName, string lastName)
    {
        var user = new AppUser
        {
            Id = userId,
            UserName = $"{userId}@test.com",
            Email = $"{userId}@test.com",
            FirstName = firstName,
            LastName = lastName
        };

        return new CompanyMember
        {
            Id = memberId,
            UserId = userId,
            User = user,
            CompanyId = companyId,
            RoleId = role.Id,
            Role = role
        };
    }

    internal static CompanyRole CreateRole(string roleName)
    {
        return new CompanyRole
        {
            Id = $"role-{roleName}",
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant()
        };
    }

    internal sealed record AuthorizationScenario(
        Company Company,
        Company OtherCompany,
        CompanyMember Admin,
        CompanyMember ProjectManager,
        CompanyMember Developer,
        CompanyMember Submitter,
        CompanyMember OtherDeveloper,
        CompanyMember ExternalDeveloper,
        Project Project,
        Project OtherProject,
        Ticket Ticket);

    private sealed class StubFileUploadService : IFileUploadService
    {
        public Task<(string url, string key)> UploadAvatarAsync(IFormFile file, string? oldKey = null)
        {
            throw new NotSupportedException();
        }

        public Task<(string url, string key)> UploadCompanyLogoAsync(IFormFile file, string? oldKey = null)
        {
            throw new NotSupportedException();
        }

        public Task<(string url, string key)> UploadAttachmentAsync(IFormFile file)
        {
            throw new NotSupportedException();
        }

        public Task DeleteFileAsync(string key)
        {
            throw new NotSupportedException();
        }
    }
}
