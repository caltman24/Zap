namespace Zap.Tests.Features.Projects;

public sealed class AddMembersTests : TicketIntegrationTestBase
{
    public AddMembersTests() : base(false)
    {
    }

    [Fact]
    public async Task AddMembers_AsAdmin_Returns_204_NoContent_And_Assigns_Members()
    {
        var (_, project, _, admin, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PostAsJsonAsync($"/projects/{project.Id}/members", new
        {
            MemberIds = new[] { submitter.Id }
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedProject = await _db.Projects
            .Include(x => x.AssignedMembers)
            .SingleAsync(x => x.Id == project.Id);
        Assert.Contains(updatedProject.AssignedMembers, member => member.Id == submitter.Id);
    }

    [Fact]
    public async Task AddMembers_With_Member_From_Another_Company_Returns_400_BadRequest()
    {
        var (_, project, _, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var otherUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherUserId);
        var otherUser = await _db.Users.FindAsync(otherUserId);
        Assert.NotNull(otherUser);
        var otherCompany = await CompanyTestData.CreateTestCompanyAsync(_db, otherUserId, otherUser);
        var outsiderId = await _db.CompanyMembers
            .Where(member => member.UserId == otherUserId && member.CompanyId == otherCompany.Id)
            .Select(member => member.Id)
            .SingleAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PostAsJsonAsync($"/projects/{project.Id}/members", new
        {
            MemberIds = new[] { outsiderId }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
