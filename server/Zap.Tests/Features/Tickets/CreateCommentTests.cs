namespace Zap.Tests.Features.Tickets;

public sealed class CreateCommentTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task CreateComment_AsAssignedDeveloper_Returns_204_NoContent_And_Persists_Comment()
    {
        var (_, _, ticket, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PostAsJsonAsync($"/tickets/{ticket.Id}/comments", new
        {
            Message = "Developer note"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var comment = await _db.TicketComments.SingleAsync(x => x.TicketId == ticket.Id);
        Assert.Equal("Developer note", comment.Message);
        Assert.Equal(developer.Id, comment.SenderId);
    }

    [Fact]
    public async Task CreateComment_AsDeveloper_Not_Assigned_To_Ticket_Returns_403_Forbidden()
    {
        var (company, project, ticket, _, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var otherDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherDeveloperUserId);
        var otherDeveloper = await _tickets.AddCompanyMemberAsync(company.Id, otherDeveloperUserId, RoleNames.Developer,
            saveChanges: false);
        otherDeveloper.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherDeveloperUserId, RoleNames.Developer);
        var response = await client.PostAsJsonAsync($"/tickets/{ticket.Id}/comments", new
        {
            Message = "Blocked note"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(await _db.TicketComments.Where(x => x.TicketId == ticket.Id).ToListAsync());
    }
}
