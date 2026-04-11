namespace Zap.Tests.Features.Tickets;

public sealed class UpdateStatusTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task UpdateStatus_AsAdmin_ReturnsSuccess()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/status", new { Status = "In Development" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsProjectManager_ReturnsSuccess()
    {
        var (_, _, ticket, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/status", new { Status = "Testing" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsDeveloper_WhenAssigned_ReturnsSuccess()
    {
        var (_, _, ticket, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/status", new { Status = "In Development" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsDeveloper_WhenNotAssigned_ReturnsForbidden()
    {
        var (company, _, ticket, _, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var otherDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherDeveloperUserId);
        await _tickets.AddCompanyMemberAsync(company.Id, otherDeveloperUserId, RoleNames.Developer);

        var client = _app.CreateClient(otherDeveloperUserId, RoleNames.Developer);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/status", new { Status = "In Development" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_AsSubmitter_WhenOwner_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/status", new { Status = "Resolved" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_OnArchivedTicket_ReturnsBadRequest()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/status", new { Status = "In Development" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}