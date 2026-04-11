namespace Zap.Tests.Features.Projects;

public sealed class CreateProjectTests : TicketIntegrationTestBase
{
    public CreateProjectTests() : base(false)
    {
    }

    [Fact]
    public async Task CreateProject_AsAdmin_Returns_201_Created_And_Persists_Project()
    {
        var (company, _, _, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var dueDate = DateTime.UtcNow.AddDays(21);
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PostAsJsonAsync("/projects", new
        {
            Name = "Platform Roadmap",
            Description = "Major delivery plan",
            Priority = "Urgent",
            DueDate = dueDate
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.NotNull(payload);
        Assert.Equal("Platform Roadmap", payload!.Name);
        Assert.Equal(company.Id, payload.CompanyId);

        _db.ChangeTracker.Clear();
        var createdProject = await _db.Projects.SingleAsync(project => project.Id == payload.Id);
        Assert.Equal("Platform Roadmap", createdProject.Name);
        Assert.Equal("Major delivery plan", createdProject.Description);
        Assert.Equal("Urgent", createdProject.Priority);
        Assert.Equal(company.Id, createdProject.CompanyId);
    }

    [Fact]
    public async Task CreateProject_AsProjectManager_Returns_201_Created()
    {
        var (_, _, _, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PostAsJsonAsync("/projects", new
        {
            Name = "PM Owned Project",
            Description = "Project manager created project",
            Priority = "High",
            DueDate = DateTime.UtcNow.AddDays(14)
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_AsDeveloper_Returns_403_Forbidden()
    {
        var (_, _, _, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PostAsJsonAsync("/projects", new
        {
            Name = "Blocked Project",
            Description = "Should not be created",
            Priority = "High",
            DueDate = DateTime.UtcNow.AddDays(14)
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
