namespace Zap.Tests.Features.Projects;

public sealed class GetAssignableProjectManagersTests : TicketIntegrationTestBase
{
    public GetAssignableProjectManagersTests() : base(false)
    {
    }

    [Fact]
    public async Task Get_Assignable_Project_Managers_Returns_All_Project_Managers_With_Assignment_Flag()
    {
        var (company, project, _, admin, pm, _, _) = await _tickets.SetupTestScenarioAsync();

        var otherPmUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherPmUserId);
        var otherPm = await _tickets.AddCompanyMemberAsync(company.Id, otherPmUserId, RoleNames.ProjectManager);

        var client = _app.CreateClient(admin.UserId);
        var response = await client.GetFromJsonAsync<List<ProjectManagerDto>>($"/projects/{project.Id}/assignable-pms");

        Assert.NotNull(response);
        Assert.Contains(response!, projectManager => projectManager.Id == pm.Id && projectManager.Assigned);
        Assert.Contains(response, projectManager => projectManager.Id == otherPm.Id && !projectManager.Assigned);
    }
}
