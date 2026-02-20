using System.Net;
using System.Net.Http.Json;
using Zap.Api.Data;
using Zap.Api.Data.Models;
using Zap.Tests.IntegrationTests;
using Microsoft.EntityFrameworkCore;
using Zap.Api.Common.Constants;

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

    private async Task<(Company company, Project project, Ticket ticket, CompanyMember admin, CompanyMember pm, CompanyMember developer, CompanyMember submitter)> SetupTestScenario()
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
            RoleId = await _db.CompanyRoles.Where(r => r.Name == RoleNames.ProjectManager).Select(r => r.Id).FirstAsync()
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

    #endregion

    #region Update Status Tests

    [Fact]
    public async Task UpdateStatus_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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
    public async Task UpdateStatus_AsSubmitter_WhenOwner_ReturnsSuccess()
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
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_OnArchivedTicket_ReturnsBadRequest()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        
        // Archive the ticket
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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
        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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
    public async Task UpdatePriority_AsSubmitter_WhenOwner_ReturnsSuccess()
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
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_OnArchivedTicket_ReturnsBadRequest()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        
        // Archive the ticket
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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
        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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
    public async Task UpdateType_AsSubmitter_WhenOwner_ReturnsSuccess()
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
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region Delete Ticket Tests

    [Fact]
    public async Task DeleteTicket_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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

        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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
        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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

        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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

        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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
        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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
    public async Task UpdateTicket_OnArchivedTicket_OnlyNameDescriptionAllowed_ReturnsSuccess()
    {
        // Arrange
        var (company, project, ticket, admin, pm, developer, submitter) = await SetupTestScenario();
        
        // Archive the ticket
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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

        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

        // Act
        var response = await client.PutAsJsonAsync(
            $"/tickets/{ticket.Id}",
            new
            {
                Name = ticket.Name,
                Description = ticket.Description,
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

        var client = _app.CreateClient(admin.UserId, RoleNames.Admin);

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
