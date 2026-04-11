namespace Zap.Tests.Features.Tickets;

public sealed class CreateTicketProjectListTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task CreateTicketProjectList_AsSubmitter_Returns_Assigned_Active_Projects_Only()
    {
        var (company, project, _, admin, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        submitter.AssignedProjects.Add(project);

        var hiddenProject = await _tickets.CreateProjectAsync(company.Id, admin.Id, "Hidden Project");
        var archivedProject = await _tickets.CreateProjectAsync(company.Id, admin.Id, "Archived Project", true);
        submitter.AssignedProjects.Add(archivedProject);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);
        var response = await client.GetFromJsonAsync<List<BasicProjectDto>>("/tickets/project-list");

        Assert.NotNull(response);
        Assert.Contains(response!, current => current.Id == project.Id);
        Assert.DoesNotContain(response, current => current.Id == hiddenProject.Id);
        Assert.DoesNotContain(response, current => current.Id == archivedProject.Id);
    }
}
