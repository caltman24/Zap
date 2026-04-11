namespace Zap.Tests.Features.Tickets;

public sealed class UpdateTypeTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task UpdateType_AsAdmin_ReturnsSuccess()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/type", new { Type = "Feature" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateType_AsDeveloper_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/type", new { Type = "Feature" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateType_AsSubmitter_WhenOwner_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/type", new { Type = "Enhancement" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}