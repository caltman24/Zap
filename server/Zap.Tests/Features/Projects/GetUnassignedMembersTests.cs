namespace Zap.Tests.Features.Projects;

public sealed class GetUnassignedMembersTests : TicketIntegrationTestBase
{
    public GetUnassignedMembersTests() : base(false)
    {
    }

    [Fact]
    public async Task Get_Unassigned_Members_Returns_Unassigned_Submitters_And_Developers_Grouped_By_Role()
    {
        var (company, project, _, admin, _, developer, submitter) = await _tickets.SetupTestScenarioAsync();

        var otherDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherDeveloperUserId);
        var otherDeveloper = await _tickets.AddCompanyMemberAsync(company.Id, otherDeveloperUserId, RoleNames.Developer);

        var client = _app.CreateClient(admin.UserId);
        var response = await client.GetFromJsonAsync<SortedDictionary<string, List<MemberInfoDto>>>(
            $"/projects/{project.Id}/members/unassigned");

        Assert.NotNull(response);
        Assert.True(response!.ContainsKey(RoleNames.Submitter));
        Assert.True(response.ContainsKey(RoleNames.Developer));
        Assert.Contains(response[RoleNames.Submitter], member => member.Id == submitter.Id);
        Assert.Contains(response[RoleNames.Developer], member => member.Id == otherDeveloper.Id);
        Assert.DoesNotContain(response[RoleNames.Developer], member => member.Id == developer.Id);
    }
}
