namespace Zap.Tests.Features.Tickets;

public sealed class DeleteTicketTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task DeleteTicket_AsAdmin_ReturnsSuccess()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_AsProjectManager_ReturnsSuccess()
    {
        var (_, _, ticket, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_AsDeveloper_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_AsSubmitter_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTicket_OnArchivedTicket_ReturnsBadRequest()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}