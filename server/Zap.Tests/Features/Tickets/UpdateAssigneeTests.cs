namespace Zap.Tests.Features.Tickets;

public sealed class UpdateAssigneeTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task UpdateAssignee_AsAdmin_ReturnsSuccess()
    {
        var (company, project, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var newDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(newDeveloperUserId);
        var newDeveloper =
            await _tickets.AddCompanyMemberAsync(company.Id, newDeveloperUserId, RoleNames.Developer,
                saveChanges: false);
        newDeveloper.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response =
            await client.PutAsJsonAsync($"/tickets/{ticket.Id}/developer", new { DeveloperId = newDeveloper.Id });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAssignee_AsProjectManager_ReturnsSuccess()
    {
        var (_, _, ticket, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsync($"/tickets/{ticket.Id}/developer", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAssignee_AsDeveloper_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PutAsync($"/tickets/{ticket.Id}/developer", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAssignee_AsSubmitter_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);

        var response = await client.PutAsync($"/tickets/{ticket.Id}/developer", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}