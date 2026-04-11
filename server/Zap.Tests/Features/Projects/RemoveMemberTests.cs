namespace Zap.Tests.Features.Projects;

public sealed class RemoveMemberTests : TicketIntegrationTestBase
{
    public RemoveMemberTests() : base(false)
    {
    }

    [Fact]
    public async Task RemoveMember_AsAdmin_Returns_204_NoContent_And_Unassigns_Developer_From_Project_Tickets()
    {
        var (_, project, ticket, admin, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.DeleteAsync($"/projects/{project.Id}/members/{developer.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedProject = await _db.Projects
            .Include(x => x.AssignedMembers)
            .SingleAsync(x => x.Id == project.Id);
        var updatedTicket = await _db.Tickets.SingleAsync(x => x.Id == ticket.Id);
        Assert.DoesNotContain(updatedProject.AssignedMembers, member => member.Id == developer.Id);
        Assert.Null(updatedTicket.AssigneeId);
    }

    [Fact]
    public async Task RemoveMember_When_Member_Does_Not_Exist_Returns_404_NotFound()
    {
        var (_, project, _, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.DeleteAsync($"/projects/{project.Id}/members/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
