namespace Zap.Tests.Features.Tickets;

public sealed class ArchiveTicketTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task ArchiveTicket_AsAdmin_ReturnsSuccess()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets.FindAsync(ticket.Id);
        Assert.NotNull(updatedTicket);
        Assert.True(updatedTicket.IsArchived);
    }

    [Fact]
    public async Task ArchiveTicket_AsProjectManager_WhenAssignedToProject_ReturnsSuccess()
    {
        var (_, _, ticket, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveTicket_AsDeveloper_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UnarchiveTicket_AsAdmin_ReturnsSuccess()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets.FindAsync(ticket.Id);
        Assert.NotNull(updatedTicket);
        Assert.False(updatedTicket.IsArchived);
    }

    [Fact]
    public async Task UnarchiveTicket_WhenProjectArchived_ReturnsBadRequest()
    {
        var (_, project, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        project.IsArchived = true;
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsync($"/tickets/{ticket.Id}/archive", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}