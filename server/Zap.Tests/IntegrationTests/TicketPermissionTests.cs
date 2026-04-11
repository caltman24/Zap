using System.Text.Json;
using Zap.Api.Common.Enums;
using Zap.Api.Features.Tickets.Services;

namespace Zap.Tests.IntegrationTests;

public class TicketPermissionTests : IAsyncDisposable
{
    private readonly ZapApplication _app;
    private readonly AppDbContext _db;

    public TicketPermissionTests()
    {
        _app = new ZapApplication();
        _db = _app.CreateAppDbContext();
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
        await _db.DisposeAsync();
    }

    #region Helper Methods

    private async
        Task<(Company company, Project project, Ticket ticket, CompanyMember admin, CompanyMember pm, CompanyMember
            developer, CompanyMember submitter)> SetupTestScenario()
    {
        // Create users
        var adminUserId = Guid.NewGuid().ToString();
        var pmUserId = Guid.NewGuid().ToString();
        var devUserId = Guid.NewGuid().ToString();
        var submitterUserId = Guid.NewGuid().ToString();

        await _app.CreateUserAsync(adminUserId);
        await _app.CreateUserAsync(pmUserId);
        await _app.CreateUserAsync(devUserId);
        await _app.CreateUserAsync(submitterUserId);

        var adminUser = await _db.Users.FindAsync(adminUserId);
        var pmUser = await _db.Users.FindAsync(pmUserId);
        var devUser = await _db.Users.FindAsync(devUserId);
        var submitterUser = await _db.Users.FindAsync(submitterUserId);

        Assert.NotNull(adminUser);
        Assert.NotNull(pmUser);
        Assert.NotNull(devUser);
        Assert.NotNull(submitterUser);

        // Create company with admin
        var company = await CompaniesTests.CreateTestCompany(
            _db,
            adminUserId,
            adminUser,
            role: RoleNames.Admin
        );

        var admin = await _db.CompanyMembers
            .FirstAsync(cm => cm.UserId == adminUserId && cm.CompanyId == company.Id);

        // Add PM
        var pm = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = pmUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.ProjectManager).Select(r => r.Id)
                .FirstAsync()
        };
        _db.CompanyMembers.Add(pm);

        // Add Developer
        var developer = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = devUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.Developer).Select(r => r.Id).FirstAsync()
        };
        _db.CompanyMembers.Add(developer);

        // Add Submitter
        var submitter = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = submitterUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.Submitter).Select(r => r.Id).FirstAsync()
        };
        _db.CompanyMembers.Add(submitter);

        // Create project with PM
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
        _db.Projects.Add(project);

        // Add developer to project (via navigation property)
        developer.AssignedProjects.Add(project);

        // Create ticket submitted by submitter, assigned to developer
        var ticket = new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Ticket",
            Description = "Test Description",
            ProjectId = project.Id,
            SubmitterId = submitter.Id,
            AssigneeId = developer.Id,
            PriorityId = await _db.TicketPriorities.Where(p => p.Name == "Low").Select(p => p.Id).FirstAsync(),
            StatusId = await _db.TicketStatuses.Where(s => s.Name == "New").Select(s => s.Id).FirstAsync(),
            TypeId = await _db.TicketTypes.Where(t => t.Name == "Defect").Select(t => t.Id).FirstAsync(),
            IsArchived = false
        };
        _db.Tickets.Add(ticket);

        await _db.SaveChangesAsync();

        return (company, project, ticket, admin, pm, developer, submitter);
    }

    private async
        Task<(Company company, Project project, Ticket ticket, CompanyMember admin, CompanyMember projectPm,
            CompanyMember submitterPm, CompanyMember developer)> SetupProjectManagerSubmitterOutsideProjectScenario()
    {
        var (company, project, ticket, admin, projectPm, developer, _) = await SetupTestScenario();

        var submitterPmUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(submitterPmUserId);

        var submitterPm = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = submitterPmUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.ProjectManager).Select(r => r.Id)
                .FirstAsync()
        };

        _db.CompanyMembers.Add(submitterPm);
        ticket.SubmitterId = submitterPm.Id;

        await _db.SaveChangesAsync();

        return (company, project, ticket, admin, projectPm, submitterPm, developer);
    }

    private async Task<Ticket> CreateTicketAsync(string projectId, string submitterId, string? assigneeId = null,
        string name = "Follow-up Ticket")
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"{name} description",
            ProjectId = projectId,
            SubmitterId = submitterId,
            AssigneeId = assigneeId,
            PriorityId = await _db.TicketPriorities.Where(p => p.Name == "Low").Select(p => p.Id).FirstAsync(),
            StatusId = await _db.TicketStatuses.Where(s => s.Name == "New").Select(s => s.Id).FirstAsync(),
            TypeId = await _db.TicketTypes.Where(t => t.Name == "Defect").Select(t => t.Id).FirstAsync(),
            IsArchived = false
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        return ticket;
    }

    private async Task AddHistoryAsync(string ticketId, string creatorId, TicketHistoryTypes type, DateTime createdAt,
        string? oldValue = null, string? newValue = null, string? relatedEntityName = null)
    {
        _db.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticketId,
            CreatorId = creatorId,
            Type = type,
            OldValue = oldValue,
            NewValue = newValue,
            RelatedEntityName = relatedEntityName,
            CreatedAt = createdAt
        });

        await _db.SaveChangesAsync();
    }

    private async Task AddCommentAsync(string ticketId, string senderId, string message, DateTime createdAt)
    {
        _db.TicketComments.Add(new TicketComment
        {
            TicketId = ticketId,
            SenderId = senderId,
            Message = message,
            CreatedAt = createdAt
        });

        await _db.SaveChangesAsync();
    }

    #endregion

    #region Read Ticket Tests

    [Fact]
    public async Task GetTicket_AsDeveloperAssignedToProject_ReturnsSuccess()
    {
        var (_, _, ticket, _, _, developer, _) = await SetupTestScenario();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.GetAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(BasicTicketDto.FormatDisplayId(ticket.Id),
            payload.RootElement.GetProperty("displayId").GetString());
    }

    [Fact]
    public async Task GetTicket_AsSubmitterAssignedToProject_ReturnsSuccess()
    {
        var (company, project, ticket, _, _, _, _) = await SetupTestScenario();

        var otherSubmitterUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherSubmitterUserId);
        var otherSubmitter = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = otherSubmitterUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.Submitter).Select(r => r.Id).FirstAsync()
        };
        otherSubmitter.AssignedProjects.Add(project);
        _db.CompanyMembers.Add(otherSubmitter);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherSubmitterUserId, RoleNames.Submitter);

        var response = await client.GetAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTicket_AsDeveloperOutsideProject_ReturnsForbidden()
    {
        var (company, _, ticket, _, _, _, _) = await SetupTestScenario();

        var otherDevUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherDevUserId);
        _db.CompanyMembers.Add(new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = otherDevUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.Developer).Select(r => r.Id).FirstAsync()
        });
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherDevUserId, RoleNames.Developer);

        var response = await client.GetAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetOpenTickets_AsDeveloperAssignedToProject_ReturnsProjectTicketsOnly()
    {
        var (company, project, ticket, _, _, developer, submitter) = await SetupTestScenario();

        var otherProject = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Other Project",
            Description = "Other Project Description",
            Priority = "High",
            CompanyId = company.Id,
            DueDate = DateTime.UtcNow.AddDays(7),
            IsArchived = false
        };
        _db.Projects.Add(otherProject);

        var otherTicket = new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Other Ticket",
            Description = "Other Description",
            ProjectId = otherProject.Id,
            SubmitterId = submitter.Id,
            PriorityId = await _db.TicketPriorities.Where(p => p.Name == "Low").Select(p => p.Id).FirstAsync(),
            StatusId = await _db.TicketStatuses.Where(s => s.Name == "New").Select(s => s.Id).FirstAsync(),
            TypeId = await _db.TicketTypes.Where(t => t.Name == "Defect").Select(t => t.Id).FirstAsync(),
            IsArchived = false
        };
        _db.Tickets.Add(otherTicket);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.GetFromJsonAsync<List<BasicTicketDto>>("/tickets/open");

        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal(ticket.Id, response[0].Id);
        Assert.Equal(project.Id, response[0].ProjectId);
    }

    [Fact]
    public async Task GetRecentActivity_AsAdmin_ReturnsLatestFiveNonCommentEvents()
    {
        var (_, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var now = DateTime.UtcNow;
        var secondTicket = await CreateTicketAsync(project.Id, submitter.Id, developer.Id, "Second Ticket");

        await AddHistoryAsync(ticket.Id, submitter.Id, TicketHistoryTypes.Created, now.AddMinutes(-6));
        await AddHistoryAsync(ticket.Id, pm.Id, TicketHistoryTypes.UpdatePriority, now.AddMinutes(-5), "Low", "High");
        await AddHistoryAsync(ticket.Id, developer.Id, TicketHistoryTypes.UpdateStatus, now.AddMinutes(-4), "New",
            "Testing");
        await AddHistoryAsync(secondTicket.Id, pm.Id, TicketHistoryTypes.DeveloperAssigned, now.AddMinutes(-3),
            relatedEntityName: "Assigned Dev");
        await AddHistoryAsync(secondTicket.Id, pm.Id, TicketHistoryTypes.Resolved, now.AddMinutes(-2));
        await AddHistoryAsync(secondTicket.Id, pm.Id, TicketHistoryTypes.DeveloperRemoved, now.AddMinutes(-1),
            relatedEntityName: "Assigned Dev");
        await AddCommentAsync(ticket.Id, admin.Id, "Hidden admin comment", now);

        var client = _app.CreateClient(admin.UserId);

        var response = await client.GetFromJsonAsync<List<RecentActivityDto>>("/tickets/recent-activity");

        Assert.NotNull(response);
        Assert.Equal(5, response.Count);
        Assert.DoesNotContain(response, activity => activity.Type == RecentActivityTypes.CommentAdded);
        Assert.Equal(
            response.OrderByDescending(activity => activity.OccurredAt).Select(activity => activity.Id),
            response.Select(activity => activity.Id));
        Assert.Equal(RecentActivityTypes.AssigneeChanged, response[0].Type);
    }

    [Fact]
    public async Task GetRecentActivity_AsDeveloper_ReturnsProjectLifecycleAndAssignedCommentsOnly()
    {
        var (company, project, ticket, _, pm, developer, submitter) = await SetupTestScenario();
        var now = DateTime.UtcNow;

        var otherDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherDeveloperUserId);
        var otherDeveloper = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = otherDeveloperUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.Developer).Select(r => r.Id).FirstAsync()
        };
        otherDeveloper.AssignedProjects.Add(project);
        _db.CompanyMembers.Add(otherDeveloper);
        await _db.SaveChangesAsync();

        var otherTicket = await CreateTicketAsync(project.Id, submitter.Id, otherDeveloper.Id, "Project Ticket");

        await AddHistoryAsync(otherTicket.Id, pm.Id, TicketHistoryTypes.UpdatePriority, now.AddMinutes(-2), "Low",
            "Urgent");
        await AddCommentAsync(otherTicket.Id, pm.Id, "Not assigned comment", now.AddMinutes(-1));
        await AddCommentAsync(ticket.Id, pm.Id, "Assigned ticket comment", now);

        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.GetFromJsonAsync<List<RecentActivityDto>>("/tickets/recent-activity");

        Assert.NotNull(response);
        Assert.Contains(response, activity =>
            activity.TicketId == otherTicket.Id && activity.Type == RecentActivityTypes.PriorityChanged);
        Assert.DoesNotContain(response, activity =>
            activity.TicketId == otherTicket.Id && activity.Type == RecentActivityTypes.CommentAdded);
        Assert.Contains(response, activity =>
            activity.TicketId == ticket.Id && activity.Type == RecentActivityTypes.CommentAdded &&
            activity.Message == "Assigned ticket comment");
    }

    [Fact]
    public async Task GetRecentActivity_AsSubmitter_ReturnsProjectLifecycleAndOwnTicketCommentsOnly()
    {
        var (company, project, ticket, _, pm, developer, submitter) = await SetupTestScenario();
        var now = DateTime.UtcNow;

        submitter.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var otherTicket = await CreateTicketAsync(project.Id, developer.Id, developer.Id, "Shared Project Ticket");

        await AddHistoryAsync(otherTicket.Id, pm.Id, TicketHistoryTypes.UpdateStatus, now.AddMinutes(-2), "New",
            "In Development");
        await AddCommentAsync(otherTicket.Id, pm.Id, "Project comment", now.AddMinutes(-1));
        await AddCommentAsync(ticket.Id, pm.Id, "Own ticket comment", now);

        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        var response = await client.GetFromJsonAsync<List<RecentActivityDto>>("/tickets/recent-activity");

        Assert.NotNull(response);
        Assert.Contains(response, activity =>
            activity.TicketId == otherTicket.Id && activity.Type == RecentActivityTypes.StatusChanged);
        Assert.DoesNotContain(response, activity =>
            activity.TicketId == otherTicket.Id && activity.Type == RecentActivityTypes.CommentAdded);
        Assert.Contains(response, activity =>
            activity.TicketId == ticket.Id && activity.Type == RecentActivityTypes.CommentAdded &&
            activity.Message == "Own ticket comment");
    }

    #endregion

    #region Update Status Tests

    [Fact]
    public async Task UpdateStatus_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/status",
            new { Status = "In Development" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsProjectManager_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/status",
            new { Status = "Testing" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsDeveloper_WhenAssigned_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/status",
            new { Status = "In Development" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsDeveloper_WhenNotAssigned_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();

        // Create another developer not assigned to ticket
        var otherDevUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherDevUserId);
        var otherDevUser = await _db.Users.FindAsync(otherDevUserId);
        Assert.NotNull(otherDevUser);

        var otherDev = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = otherDevUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.Developer).Select(r => r.Id).FirstAsync()
        };
        _db.CompanyMembers.Add(otherDev);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherDevUserId, RoleNames.Developer);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/status",
            new { Status = "In Development" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsSubmitter_WhenOwner_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/status",
            new { Status = "Resolved" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_OnArchivedTicket_ReturnsBadRequest()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();

        // Archive the ticket
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/status",
            new { Status = "In Development" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Priority Tests

    [Fact]
    public async Task UpdatePriority_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/priority",
            new { Priority = "High" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_AsProjectManager_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/priority",
            new { Priority = "Urgent" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_AsDeveloper_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/priority",
            new { Priority = "High" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_AsSubmitter_WhenOwner_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/priority",
            new { Priority = "Medium" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_OnArchivedTicket_ReturnsBadRequest()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();

        // Archive the ticket
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/priority",
            new { Priority = "High" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Type Tests

    [Fact]
    public async Task UpdateType_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/type",
            new { Type = "Feature" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateType_AsDeveloper_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/type",
            new { Type = "Feature" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateType_AsSubmitter_WhenOwner_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/type",
            new { Type = "Enhancement" }
        );

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Delete Ticket Tests

    [Fact]
    public async Task DeleteTicket_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_AsProjectManager_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        // Act
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_AsDeveloper_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        // Act
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_AsSubmitter_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        // Act
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_OnArchivedTicket_ReturnsBadRequest()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();

        // Archive the ticket
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Archive Ticket Tests

    [Fact]
    public async Task ArchiveTicket_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify ticket is archived
        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets.FindAsync(ticket.Id);
        Assert.NotNull(updatedTicket);
        Assert.True(updatedTicket.IsArchived);
    }

    [Fact]
    public async Task ArchiveTicket_AsProjectManager_WhenAssignedToProject_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        // Act
        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveTicket_AsDeveloper_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        // Act
        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UnarchiveTicket_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();

        // Archive the ticket first
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify ticket is unarchived
        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets.FindAsync(ticket.Id);
        Assert.NotNull(updatedTicket);
        Assert.False(updatedTicket.IsArchived);
    }

    [Fact]
    public async Task UnarchiveTicket_WhenProjectArchived_ReturnsBadRequest()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();

        // Archive both project and ticket
        project.IsArchived = true;
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Ticket (Full) Tests

    [Fact]
    public async Task UpdateTicket_AsAdmin_UpdatesAllFields_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}",
            new
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Priority = "High",
                Status = "In Development",
                Type = "Feature"
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify all fields updated
        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Type)
            .FirstAsync(t => t.Id == ticket.Id);
        Assert.Equal("Updated Name", updatedTicket.Name);
        Assert.Equal("Updated Description", updatedTicket.Description);
        Assert.Equal("High", updatedTicket.Priority.Name);
        Assert.Equal("In Development", updatedTicket.Status.Name);
        Assert.Equal("Feature", updatedTicket.Type.Name);
    }

    [Fact]
    public async Task UpdateTicket_AsDeveloper_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}",
            new
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Priority = "High",
                Status = "In Development",
                Type = "Feature"
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTicket_AsSubmitterProjectManagerOutsideProject_UpdatesNameDescriptionOnly_ReturnsSuccess()
    {
        // Arrange
        var (_, _, ticket, _, _, submitterPm, _) = await SetupProjectManagerSubmitterOutsideProjectScenario();
        var client = _app.CreateClient(submitterPm.UserId, RoleNames.ProjectManager);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}",
            new
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Priority = "Low",
                Status = "New",
                Type = "Defect"
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .Include(t => t.Type)
            .FirstAsync(t => t.Id == ticket.Id);
        Assert.Equal("Updated Name", updatedTicket.Name);
        Assert.Equal("Updated Description", updatedTicket.Description);
        Assert.Equal("Low", updatedTicket.Priority.Name);
        Assert.Equal("New", updatedTicket.Status.Name);
        Assert.Equal("Defect", updatedTicket.Type.Name);
    }

    [Fact]
    public async Task UpdateTicket_AsSubmitterProjectManagerOutsideProject_ChangingStatus_ReturnsBadRequest()
    {
        // Arrange
        var (_, _, ticket, _, _, submitterPm, _) = await SetupProjectManagerSubmitterOutsideProjectScenario();
        var client = _app.CreateClient(submitterPm.UserId, RoleNames.ProjectManager);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}",
            new
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Priority = "Low",
                Status = "In Development",
                Type = "Defect"
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTicket_OnArchivedTicket_OnlyNameDescriptionAllowed_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();

        // Archive the ticket
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}",
            new
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Priority = "Low", // Same as original
                Status = "New", // Same as original
                Type = "Defect" // Same as original
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify only name/description updated
        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets.FindAsync(ticket.Id);
        Assert.NotNull(updatedTicket);
        Assert.Equal("Updated Name", updatedTicket.Name);
        Assert.Equal("Updated Description", updatedTicket.Description);
    }

    [Fact]
    public async Task UpdateTicket_OnArchivedTicket_ChangingStatus_ReturnsBadRequest()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();

        // Archive the ticket
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}",
            new
            {
                ticket.Name,
                ticket.Description,
                Priority = "Low",
                Status = "In Development", // Trying to change status
                Type = "Defect"
            }
        );

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Assignee Tests

    [Fact]
    public async Task UpdateAssignee_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();

        // Create another developer to assign
        var newDevUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(newDevUserId);
        var newDevUser = await _db.Users.FindAsync(newDevUserId);
        Assert.NotNull(newDevUser);

        var newDev = new CompanyMember
        {
            Id = Guid.NewGuid().ToString(),
            UserId = newDevUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.Developer).Select(r => r.Id).FirstAsync()
        };
        _db.CompanyMembers.Add(newDev);

        // Add to project via navigation property
        newDev.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}/developer",
            new { DeveloperId = newDev.Id }
        );

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAssignee_AsProjectManager_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        // Act - Unassign developer
        var response = await client.PutAsync($"/tickets/{ticket.Id}/developer", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAssignee_AsDeveloper_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        // Act
        var response = await client.PutAsync($"/tickets/{ticket.Id}/developer", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAssignee_AsSubmitter_ReturnsForbidden()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        // Act
        var response = await client.PutAsync($"/tickets/{ticket.Id}/developer", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion
}