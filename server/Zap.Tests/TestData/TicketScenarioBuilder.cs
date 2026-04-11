namespace Zap.Tests.TestData;

public sealed class TicketScenarioBuilder(Zap.Tests.Infrastructure.ZapApplication app, AppDbContext db)
{
    public async Task<(Company company, Project project, Ticket ticket, CompanyMember admin, CompanyMember pm,
        CompanyMember developer, CompanyMember submitter)> SetupTestScenarioAsync()
    {
        var adminUserId = Guid.NewGuid().ToString();
        var pmUserId = Guid.NewGuid().ToString();
        var developerUserId = Guid.NewGuid().ToString();
        var submitterUserId = Guid.NewGuid().ToString();

        await app.CreateUserAsync(adminUserId);
        await app.CreateUserAsync(pmUserId);
        await app.CreateUserAsync(developerUserId);
        await app.CreateUserAsync(submitterUserId);

        var adminUser = await db.Users.FindAsync(adminUserId);
        var pmUser = await db.Users.FindAsync(pmUserId);
        var developerUser = await db.Users.FindAsync(developerUserId);
        var submitterUser = await db.Users.FindAsync(submitterUserId);

        Assert.NotNull(adminUser);
        Assert.NotNull(pmUser);
        Assert.NotNull(developerUser);
        Assert.NotNull(submitterUser);

        var company = await CompanyTestData.CreateTestCompanyAsync(db, adminUserId, adminUser, role: RoleNames.Admin);

        var admin = await db.CompanyMembers.FirstAsync(member =>
            member.UserId == adminUserId && member.CompanyId == company.Id);
        var pm = await AddCompanyMemberAsync(company.Id, pmUserId, RoleNames.ProjectManager);
        var developer =
            await AddCompanyMemberAsync(company.Id, developerUserId, RoleNames.Developer, saveChanges: false);
        var submitter =
            await AddCompanyMemberAsync(company.Id, submitterUserId, RoleNames.Submitter, saveChanges: false);

        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Project",
            Description = "Test Project Description",
            Priority = "High",
            CompanyId = company.Id,
            ProjectManagerId = pm.Id,
            DueDate = DateTime.UtcNow.AddDays(30),
            IsArchived = false
        };
        db.Projects.Add(project);

        developer.AssignedProjects.Add(project);

        var ticket = new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Ticket",
            Description = "Test Description",
            ProjectId = project.Id,
            SubmitterId = submitter.Id,
            AssigneeId = developer.Id,
            PriorityId = await db.TicketPriorities.Where(priority => priority.Name == Priorities.Low)
                .Select(priority => priority.Id)
                .FirstAsync(),
            StatusId = await db.TicketStatuses.Where(status => status.Name == TicketStatuses.New)
                .Select(status => status.Id)
                .FirstAsync(),
            TypeId = await db.TicketTypes.Where(type => type.Name == TicketTypes.Defect)
                .Select(type => type.Id)
                .FirstAsync(),
            IsArchived = false
        };
        db.Tickets.Add(ticket);

        await db.SaveChangesAsync();

        return (company, project, ticket, admin, pm, developer, submitter);
    }

    public async Task<(Company company, Project project, Ticket ticket, CompanyMember admin, CompanyMember projectPm,
        CompanyMember submitterPm, CompanyMember developer)> SetupProjectManagerSubmitterOutsideProjectScenarioAsync()
    {
        var (company, project, ticket, admin, projectPm, developer, _) = await SetupTestScenarioAsync();

        var submitterPmUserId = Guid.NewGuid().ToString();
        await app.CreateUserAsync(submitterPmUserId);

        var submitterPm =
            await AddCompanyMemberAsync(company.Id, submitterPmUserId, RoleNames.ProjectManager, saveChanges: false);
        ticket.SubmitterId = submitterPm.Id;

        await db.SaveChangesAsync();

        return (company, project, ticket, admin, projectPm, submitterPm, developer);
    }

    public async Task<CompanyMember> AddCompanyMemberAsync(string companyId, string userId, string roleName,
        Project? assignedProject = null, bool saveChanges = true)
    {
        var member = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            CompanyId = companyId,
            RoleId = await db.CompanyRoles.Where(role => role.Name == roleName).Select(role => role.Id).FirstAsync()
        };

        if (assignedProject != null) member.AssignedProjects.Add(assignedProject);

        db.CompanyMembers.Add(member);

        if (saveChanges) await db.SaveChangesAsync();

        return member;
    }

    public async Task<Project> CreateProjectAsync(string companyId, string? projectManagerId = null,
        string name = "Test Project", bool isArchived = false, string priority = "High", DateTime? dueDate = null)
    {
        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"{name} description",
            Priority = priority,
            CompanyId = companyId,
            ProjectManagerId = projectManagerId,
            DueDate = dueDate ?? DateTime.UtcNow.AddDays(30),
            IsArchived = isArchived
        };

        db.Projects.Add(project);
        await db.SaveChangesAsync();

        return project;
    }

    public async Task<Ticket> CreateTicketAsync(string projectId, string submitterId, string? assigneeId = null,
        string name = "Follow-up Ticket", bool isArchived = false)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"{name} description",
            ProjectId = projectId,
            SubmitterId = submitterId,
            AssigneeId = assigneeId,
            PriorityId = await db.TicketPriorities.Where(priority => priority.Name == Priorities.Low)
                .Select(priority => priority.Id)
                .FirstAsync(),
            StatusId = await db.TicketStatuses.Where(status => status.Name == TicketStatuses.New)
                .Select(status => status.Id)
                .FirstAsync(),
            TypeId = await db.TicketTypes.Where(type => type.Name == TicketTypes.Defect)
                .Select(type => type.Id)
                .FirstAsync(),
            IsArchived = isArchived
        };

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        return ticket;
    }

    public async Task AddHistoryAsync(string ticketId, string creatorId, TicketHistoryTypes type, DateTime createdAt,
        string? oldValue = null, string? newValue = null, string? relatedEntityName = null)
    {
        db.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticketId,
            CreatorId = creatorId,
            Type = type,
            OldValue = oldValue,
            NewValue = newValue,
            RelatedEntityName = relatedEntityName,
            CreatedAt = createdAt
        });

        await db.SaveChangesAsync();
    }

    public async Task AddCommentAsync(string ticketId, string senderId, string message, DateTime createdAt)
    {
        db.TicketComments.Add(new TicketComment
        {
            TicketId = ticketId,
            SenderId = senderId,
            Message = message,
            CreatedAt = createdAt
        });

        await db.SaveChangesAsync();
    }

    public async Task<TicketComment> CreateCommentAsync(string ticketId, string senderId, string message)
    {
        var comment = new TicketComment
        {
            Id = Guid.NewGuid().ToString(),
            TicketId = ticketId,
            SenderId = senderId,
            Message = message
        };

        db.TicketComments.Add(comment);
        await db.SaveChangesAsync();

        return comment;
    }
}
