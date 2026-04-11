namespace Zap.Tests.Features.Tickets;

public sealed class UpdatePriorityTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task UpdatePriority_AsAdmin_ReturnsSuccess_And_Persists_Change()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "High" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets
            .Include(x => x.Priority)
            .FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal("High", updatedTicket.Priority.Name);

        var historyEntry = await _db.TicketHistories.SingleAsync(x => x.TicketId == ticket.Id);
        Assert.Equal(TicketHistoryTypes.UpdatePriority, historyEntry.Type);
        Assert.Equal("Low", historyEntry.OldValue);
        Assert.Equal("High", historyEntry.NewValue);
    }

    [Fact]
    public async Task UpdatePriority_AsProjectManager_ReturnsSuccess_And_Persists_Change()
    {
        var (_, _, ticket, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "Urgent" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets
            .Include(x => x.Priority)
            .FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal("Urgent", updatedTicket.Priority.Name);
    }

    [Theory]
    [InlineData(RoleNames.Developer)]
    [InlineData(RoleNames.Submitter)]
    public async Task UpdatePriority_As_Disallowed_Role_ReturnsForbidden_And_DoesNotChangePriority(string roleName)
    {
        var (_, _, ticket, _, _, developer, submitter) = await _tickets.SetupTestScenarioAsync();
        var member = roleName == RoleNames.Developer ? developer : submitter;
        var client = _app.CreateClient(member.UserId, roleName);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "High" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedTicket = await _db.Tickets
            .Include(x => x.Priority)
            .FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal("Low", unchangedTicket.Priority.Name);
    }

    [Fact]
    public async Task UpdatePriority_OnArchivedTicket_ReturnsBadRequest_And_DoesNotChangePriority()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "High" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedTicket = await _db.Tickets
            .Include(x => x.Priority)
            .FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal("Low", unchangedTicket.Priority.Name);
    }

    [Fact]
    public async Task UpdatePriority_With_Invalid_Priority_ReturnsBadRequest_And_DoesNotChangePriority()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/priority", new { Priority = "Impossible" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedTicket = await _db.Tickets
            .Include(x => x.Priority)
            .FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal("Low", unchangedTicket.Priority.Name);
    }
}
