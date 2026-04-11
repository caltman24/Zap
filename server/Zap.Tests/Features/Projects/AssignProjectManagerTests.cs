namespace Zap.Tests.Features.Projects;

public sealed class AssignProjectManagerTests : TicketIntegrationTestBase
{
    public AssignProjectManagerTests() : base(false)
    {
    }

    [Fact]
    public async Task AssignProjectManager_AsAdmin_Returns_204_NoContent_And_Updates_Project_Manager()
    {
        var (company, project, _, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var newPmUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(newPmUserId);
        var newPm = await _tickets.AddCompanyMemberAsync(company.Id, newPmUserId, RoleNames.ProjectManager);

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/projects/{project.Id}/pm", new { MemberId = newPm.Id });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedProject = await _db.Projects.SingleAsync(x => x.Id == project.Id);
        Assert.Equal(newPm.Id, updatedProject.ProjectManagerId);
    }

    [Fact]
    public async Task AssignProjectManager_With_Null_MemberId_Removes_Project_Manager()
    {
        var (_, project, _, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/projects/{project.Id}/pm", new { MemberId = (string?)null });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedProject = await _db.Projects.SingleAsync(x => x.Id == project.Id);
        Assert.Null(updatedProject.ProjectManagerId);
    }

    [Fact]
    public async Task AssignProjectManager_With_Non_Project_Manager_Returns_400_BadRequest()
    {
        var (_, project, _, admin, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/projects/{project.Id}/pm", new { MemberId = developer.Id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedProject = await _db.Projects.SingleAsync(x => x.Id == project.Id);
        Assert.NotEqual(developer.Id, unchangedProject.ProjectManagerId);
    }
}
