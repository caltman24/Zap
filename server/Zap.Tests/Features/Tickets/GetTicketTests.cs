using System.Text.Json;

namespace Zap.Tests.Features.Tickets;

public sealed class GetTicketTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task GetTicket_AsDeveloperAssignedToProject_ReturnsSuccess()
    {
        var (_, _, ticket, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
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
        var (company, project, ticket, _, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var otherSubmitterUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherSubmitterUserId);
        var otherSubmitter = await _tickets.AddCompanyMemberAsync(company.Id, otherSubmitterUserId, RoleNames.Submitter,
            saveChanges: false);
        otherSubmitter.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherSubmitterUserId, RoleNames.Submitter);
        var response = await client.GetAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTicket_AsDeveloperOutsideProject_ReturnsForbidden()
    {
        var (company, _, ticket, _, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var otherDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherDeveloperUserId);
        await _tickets.AddCompanyMemberAsync(company.Id, otherDeveloperUserId, RoleNames.Developer);

        var client = _app.CreateClient(otherDeveloperUserId, RoleNames.Developer);
        var response = await client.GetAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}