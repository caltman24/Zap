namespace Zap.Tests.Features.Tickets;

public sealed class UpdatePriorityTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task UpdatePriority_AsAdmin_ReturnsSuccess()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "High" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_AsProjectManager_ReturnsSuccess()
    {
        var (_, _, ticket, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "Urgent" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_AsDeveloper_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "High" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_AsSubmitter_WhenOwner_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "Medium" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePriority_OnArchivedTicket_ReturnsBadRequest()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "High" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}