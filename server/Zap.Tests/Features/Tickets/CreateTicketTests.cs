namespace Zap.Tests.Features.Tickets;

public sealed class CreateTicketTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task CreateTicket_AsAssignedSubmitter_Returns_201_Created_And_Persists_Ticket_And_History()
    {
        var (_, project, _, _, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        submitter.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);
        var response = await client.PostAsJsonAsync("/tickets", new
        {
            Name = "New API Ticket",
            Description = "Ticket created through the API",
            Priority = "High",
            Status = "New",
            Type = "Feature",
            ProjectId = project.Id
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CreateTicketResult>();
        Assert.NotNull(payload);

        _db.ChangeTracker.Clear();
        var createdTicket = await _db.Tickets
            .Include(x => x.Priority)
            .Include(x => x.Status)
            .Include(x => x.Type)
            .SingleAsync(x => x.Id == payload!.Id);

        Assert.Equal("New API Ticket", createdTicket.Name);
        Assert.Equal("Ticket created through the API", createdTicket.Description);
        Assert.Equal(project.Id, createdTicket.ProjectId);
        Assert.Equal(submitter.Id, createdTicket.SubmitterId);
        Assert.Equal("High", createdTicket.Priority.Name);
        Assert.Equal(TicketStatuses.New, createdTicket.Status.Name);
        Assert.Equal(TicketTypes.Feature, createdTicket.Type.Name);

        var historyEntry = await _db.TicketHistories.SingleAsync(x => x.TicketId == createdTicket.Id);
        Assert.Equal(TicketHistoryTypes.Created, historyEntry.Type);
        Assert.Equal(submitter.Id, historyEntry.CreatorId);
    }

    [Fact]
    public async Task CreateTicket_AsDeveloper_Returns_403_Forbidden()
    {
        var (_, project, _, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PostAsJsonAsync("/tickets", new
        {
            Name = "Blocked Ticket",
            Description = "Should not be created",
            Priority = "High",
            Status = "New",
            Type = "Feature",
            ProjectId = project.Id
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
