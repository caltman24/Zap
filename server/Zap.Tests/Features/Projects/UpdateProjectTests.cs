namespace Zap.Tests.Features.Projects;

public sealed class UpdateProjectTests : TicketIntegrationTestBase
{
    public UpdateProjectTests() : base(false)
    {
    }

    [Fact]
    public async Task UpdateProject_AsAdmin_Updates_All_Fields()
    {
        var (_, project, _, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var dueDate = DateTime.UtcNow.AddDays(45);
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/projects/{project.Id}", new
        {
            Name = "Updated Project",
            Description = "Updated project description",
            Priority = "Urgent",
            DueDate = dueDate
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedProject = await _db.Projects.SingleAsync(x => x.Id == project.Id);
        Assert.Equal("Updated Project", updatedProject.Name);
        Assert.Equal("Updated project description", updatedProject.Description);
        Assert.Equal("Urgent", updatedProject.Priority);
        Assert.Equal(dueDate, updatedProject.DueDate, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task UpdateProject_AsProjectManager_Updates_Own_Project()
    {
        var (_, project, _, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var dueDate = DateTime.UtcNow.AddDays(60);
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsJsonAsync($"/projects/{project.Id}", new
        {
            Name = "PM Updated Project",
            Description = "Updated by PM",
            Priority = "Medium",
            DueDate = dueDate
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedProject = await _db.Projects.SingleAsync(x => x.Id == project.Id);
        Assert.Equal("PM Updated Project", updatedProject.Name);
        Assert.Equal("Updated by PM", updatedProject.Description);
        Assert.Equal("Medium", updatedProject.Priority);
    }

    [Fact]
    public async Task UpdateProject_AsDeveloper_Returns_403_Forbidden()
    {
        var (_, project, _, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PutAsJsonAsync($"/projects/{project.Id}", new
        {
            Name = "Blocked Update",
            Description = "Blocked update description",
            Priority = "High",
            DueDate = DateTime.UtcNow.AddDays(30)
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProject_OnArchivedProject_Allows_Name_And_Description_Only()
    {
        var (_, project, _, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        project.IsArchived = true;
        project.DueDate = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/projects/{project.Id}", new
        {
            Name = "Archived Rename",
            Description = "Archived description",
            project.Priority,
            project.DueDate
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedProject = await _db.Projects.SingleAsync(x => x.Id == project.Id);
        Assert.Equal("Archived Rename", updatedProject.Name);
        Assert.Equal("Archived description", updatedProject.Description);
        Assert.Equal("High", updatedProject.Priority);
        Assert.Equal(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), updatedProject.DueDate);
    }

    [Fact]
    public async Task UpdateProject_OnArchivedProject_When_Changing_Priority_Returns_400_BadRequest()
    {
        var (_, project, _, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        project.IsArchived = true;
        project.DueDate = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/projects/{project.Id}", new
        {
            Name = "Archived Rename",
            Description = "Archived description",
            Priority = "Urgent",
            project.DueDate
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedProject = await _db.Projects.SingleAsync(x => x.Id == project.Id);
        Assert.Equal("High", unchangedProject.Priority);
    }
}
