namespace Zap.Tests.Features.Tickets;

public sealed class UpdateTypeTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task UpdateType_AsAdmin_ReturnsSuccess_And_Persists_Type()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/type", new { Type = "Feature" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets
            .Include(x => x.Type)
            .FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal(TicketTypes.Feature, updatedTicket.Type.Name);

        var historyEntry = await _db.TicketHistories.SingleAsync(x => x.TicketId == ticket.Id);
        Assert.Equal(TicketHistoryTypes.UpdateType, historyEntry.Type);
        Assert.Equal(TicketTypes.Defect, historyEntry.OldValue);
        Assert.Equal(TicketTypes.Feature, historyEntry.NewValue);
    }

    [Fact]
    public async Task UpdateType_AsProjectManager_ReturnsSuccess_And_Persists_Type()
    {
        var (_, _, ticket, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/type", new { Type = "Feature" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets
            .Include(x => x.Type)
            .FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal(TicketTypes.Feature, updatedTicket.Type.Name);
    }

    [Theory]
    [InlineData(RoleNames.Developer)]
    [InlineData(RoleNames.Submitter)]
    public async Task UpdateType_As_Disallowed_Role_ReturnsForbidden_And_DoesNotChangeType(string roleName)
    {
        var (_, _, ticket, _, _, developer, submitter) = await _tickets.SetupTestScenarioAsync();
        var member = roleName == RoleNames.Developer ? developer : submitter;
        var client = _app.CreateClient(member.UserId, roleName);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/type", new { Type = "Feature" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedTicket = await _db.Tickets
            .Include(x => x.Type)
            .FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal(TicketTypes.Defect, unchangedTicket.Type.Name);
    }

    [Fact]
    public async Task UpdateType_With_Invalid_Type_ReturnsBadRequest_And_DoesNotChangeType()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/type", new { Type = "Epic" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedTicket = await _db.Tickets
            .Include(x => x.Type)
            .FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal(TicketTypes.Defect, unchangedTicket.Type.Name);
    }
}
