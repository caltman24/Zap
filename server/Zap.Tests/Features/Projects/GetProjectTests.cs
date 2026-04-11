namespace Zap.Tests.Features.Projects;

public sealed class GetProjectTests : TicketIntegrationTestBase
{
    public GetProjectTests() : base(false)
    {
    }

    [Fact]
    public async Task Get_Project_As_Admin_Returns_Project_Details_And_Capabilities()
    {
        var (_, project, ticket, admin, pm, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.GetFromJsonAsync<ProjectResponse>($"/projects/{project.Id}");

        Assert.NotNull(response);
        Assert.Equal(project.Id, response!.Id);
        Assert.Equal(project.Name, response.Name);
        Assert.Equal(pm.Id, response.ProjectManager!.Id);
        Assert.Contains(response.Tickets, projectTicket => projectTicket.Id == ticket.Id);
        Assert.Contains(response.Members, member => member.Id == developer.Id);
        Assert.True(response.Capabilities.CanEdit);
        Assert.True(response.Capabilities.CanArchive);
        Assert.True(response.Capabilities.CanAssignProjectManager);
        Assert.True(response.Capabilities.CanManageMembers);
        Assert.True(response.Capabilities.CanCreateTicket);
    }

    [Fact]
    public async Task Get_Project_As_Developer_Outside_Project_Returns_Forbidden()
    {
        var (company, visibleProject, _, admin, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var hiddenProject = await _tickets.CreateProjectAsync(company.Id, admin.Id, "Hidden Project");

        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var visibleResponse = await client.GetAsync($"/projects/{visibleProject.Id}");
        var hiddenResponse = await client.GetAsync($"/projects/{hiddenProject.Id}");

        Assert.Equal(HttpStatusCode.OK, visibleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, hiddenResponse.StatusCode);
    }

    private sealed record ProjectResponse(
        string Id,
        string Name,
        string Description,
        string Priority,
        string CompanyId,
        MemberInfoDto? ProjectManager,
        bool IsArchived,
        DateTime DueDate,
        List<BasicTicketDto> Tickets,
        List<MemberInfoDto> Members,
        ProjectCapabilitiesDto Capabilities);
}
