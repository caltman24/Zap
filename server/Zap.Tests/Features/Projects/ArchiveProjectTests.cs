namespace Zap.Tests.Features.Projects;

public sealed class ArchiveProjectTests : TicketIntegrationTestBase
{
    public ArchiveProjectTests() : base(false)
    {
    }

    [Fact]
    public async Task ArchiveProject_AsAdmin_Returns_204_NoContent_And_Archives_Project_And_Tickets()
    {
        var (_, project, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsync($"/projects/{project.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedProject = await _db.Projects.SingleAsync(x => x.Id == project.Id);
        var updatedTicket = await _db.Tickets.SingleAsync(x => x.Id == ticket.Id);
        Assert.True(updatedProject.IsArchived);
        Assert.True(updatedTicket.IsArchived);
    }

    [Fact]
    public async Task UnarchiveProject_AsAdmin_Returns_204_NoContent_And_Unarchives_Project_And_Tickets()
    {
        var (_, project, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        project.IsArchived = true;
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsync($"/projects/{project.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedProject = await _db.Projects.SingleAsync(x => x.Id == project.Id);
        var updatedTicket = await _db.Tickets.SingleAsync(x => x.Id == ticket.Id);
        Assert.False(updatedProject.IsArchived);
        Assert.False(updatedTicket.IsArchived);
    }

    [Fact]
    public async Task ArchiveProject_AsProjectManager_Returns_204_NoContent()
    {
        var (_, project, _, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsync($"/projects/{project.Id}/archive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveProject_AsDeveloper_Returns_403_Forbidden()
    {
        var (_, project, _, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PutAsync($"/projects/{project.Id}/archive", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
