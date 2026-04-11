namespace Zap.Tests.Features.Tickets;

public sealed class UpdateTicketTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task UpdateTicket_AsAdmin_UpdatesAllFields_ReturnsSuccess()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(admin.UserId);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}", new
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Priority = "High",
            Status = "In Development",
            Type = "Feature"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets
            .Include(ticketEntity => ticketEntity.Priority)
            .Include(ticketEntity => ticketEntity.Status)
            .Include(ticketEntity => ticketEntity.Type)
            .FirstAsync(ticketEntity => ticketEntity.Id == ticket.Id);
        Assert.Equal("Updated Name", updatedTicket.Name);
        Assert.Equal("Updated Description", updatedTicket.Description);
        Assert.Equal("High", updatedTicket.Priority.Name);
        Assert.Equal("In Development", updatedTicket.Status.Name);
        Assert.Equal("Feature", updatedTicket.Type.Name);
    }

    [Fact]
    public async Task UpdateTicket_AsDeveloper_ReturnsForbidden()
    {
        var (_, _, ticket, _, _, developer, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}", new
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Priority = "High",
            Status = "In Development",
            Type = "Feature"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTicket_AsSubmitterProjectManagerOutsideProject_UpdatesNameDescriptionOnly_ReturnsSuccess()
    {
        var (_, _, ticket, _, _, submitterPm, _) =
            await _tickets.SetupProjectManagerSubmitterOutsideProjectScenarioAsync();
        var client = _app.CreateClient(submitterPm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}", new
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Priority = "Low",
            Status = "New",
            Type = "Defect"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets
            .Include(ticketEntity => ticketEntity.Priority)
            .Include(ticketEntity => ticketEntity.Status)
            .Include(ticketEntity => ticketEntity.Type)
            .FirstAsync(ticketEntity => ticketEntity.Id == ticket.Id);
        Assert.Equal("Updated Name", updatedTicket.Name);
        Assert.Equal("Updated Description", updatedTicket.Description);
        Assert.Equal("Low", updatedTicket.Priority.Name);
        Assert.Equal("New", updatedTicket.Status.Name);
        Assert.Equal("Defect", updatedTicket.Type.Name);
    }

    [Fact]
    public async Task UpdateTicket_AsSubmitterProjectManagerOutsideProject_ChangingStatus_ReturnsBadRequest()
    {
        var (_, _, ticket, _, _, submitterPm, _) =
            await _tickets.SetupProjectManagerSubmitterOutsideProjectScenarioAsync();
        var client = _app.CreateClient(submitterPm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}", new
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Priority = "Low",
            Status = "In Development",
            Type = "Defect"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTicket_OnArchivedTicket_OnlyNameDescriptionAllowed_ReturnsSuccess()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}", new
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Priority = "Low",
            Status = "New",
            Type = "Defect"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets.FindAsync(ticket.Id);
        Assert.NotNull(updatedTicket);
        Assert.Equal("Updated Name", updatedTicket.Name);
        Assert.Equal("Updated Description", updatedTicket.Description);
    }

    [Fact]
    public async Task UpdateTicket_OnArchivedTicket_ChangingStatus_ReturnsBadRequest()
    {
        var (_, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();
        ticket.IsArchived = true;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}", new
        {
            ticket.Name,
            ticket.Description,
            Priority = "Low",
            Status = "In Development",
            Type = "Defect"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}