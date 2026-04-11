namespace Zap.Tests.Features.Tickets;

public sealed class DeleteTicketTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task DeleteTicket_AsAdmin_ReturnsSuccess_And_Removes_Ticket()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        Assert.Null(await _db.Tickets.FindAsync(ticket.Id));
    }

    [Fact]
    public async Task DeleteTicket_AsProjectManager_ReturnsSuccess_And_Removes_Ticket()
    {
        var (_, _, ticket, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        Assert.Null(await _db.Tickets.FindAsync(ticket.Id));
    }

    [Theory]
    [InlineData(RoleNames.Developer)]
    [InlineData(RoleNames.Submitter)]
    public async Task DeleteTicket_As_Disallowed_Role_ReturnsForbidden_And_DoesNot_Delete_Ticket(string roleName)
    {
        var (_, _, ticket, _, _, developer, submitter) = await _tickets.SetupTestScenarioAsync();
        var member = roleName == RoleNames.Developer ? developer : submitter;
        var client = _app.CreateClient(member.UserId, roleName);

        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        _db.ChangeTracker.Clear();
        Assert.NotNull(await _db.Tickets.FindAsync(ticket.Id));
    }

    [Fact]
    public async Task DeleteTicket_OnArchivedTicket_ReturnsBadRequest_And_DoesNot_Delete_Ticket()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _db.ChangeTracker.Clear();
        Assert.NotNull(await _db.Tickets.FindAsync(ticket.Id));
    }
}
