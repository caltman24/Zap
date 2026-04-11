namespace Zap.Tests.Features.Tickets;

public sealed class UpdateAssigneeTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task UpdateAssignee_AsAdmin_ReturnsSuccess_And_Reassigns_Ticket()
    {
        var (company, project, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var newDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(newDeveloperUserId);
        var newDeveloper =
            await _tickets.AddCompanyMemberAsync(company.Id, newDeveloperUserId, RoleNames.Developer,
                saveChanges: false);
        newDeveloper.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response =
            await client.PutAsJsonAsync($"/tickets/{ticket.Id}/developer", new { DeveloperId = newDeveloper.Id });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets.FirstAsync(x => x.Id == ticket.Id);
        Assert.Equal(newDeveloper.Id, updatedTicket.AssigneeId);

        var historyEntry = await _db.TicketHistories.SingleAsync(x => x.TicketId == ticket.Id);
        Assert.Equal(TicketHistoryTypes.DeveloperAssigned, historyEntry.Type);
        Assert.Equal(newDeveloper.Id, historyEntry.RelatedEntityId);
    }

    [Fact]
    public async Task UpdateAssignee_AsProjectManager_With_Null_Request_ReturnsSuccess_And_Unassigns_Ticket()
    {
        var (_, _, ticket, _, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var client = _app.CreateClient(pm.UserId, RoleNames.ProjectManager);

        var response = await client.PutAsync($"/tickets/{ticket.Id}/developer", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedTicket = await _db.Tickets.FirstAsync(x => x.Id == ticket.Id);
        Assert.Null(updatedTicket.AssigneeId);

        var historyEntry = await _db.TicketHistories.SingleAsync(x => x.TicketId == ticket.Id);
        Assert.Equal(TicketHistoryTypes.DeveloperRemoved, historyEntry.Type);
    }

    [Theory]
    [InlineData(RoleNames.Developer)]
    [InlineData(RoleNames.Submitter)]
    public async Task UpdateAssignee_As_Disallowed_Role_ReturnsForbidden_And_DoesNotChangeAssignee(string roleName)
    {
        var (_, _, ticket, _, _, developer, submitter) = await _tickets.SetupTestScenarioAsync();
        var member = roleName == RoleNames.Developer ? developer : submitter;
        var client = _app.CreateClient(member.UserId, roleName);

        var response = await client.PutAsync($"/tickets/{ticket.Id}/developer", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedTicket = await _db.Tickets.FirstAsync(x => x.Id == ticket.Id);
        Assert.NotNull(unchangedTicket.AssigneeId);
    }

    [Fact]
    public async Task UpdateAssignee_With_Mismatched_Assignee_Fields_ReturnsBadRequest()
    {
        var (company, project, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var firstDeveloperUserId = Guid.NewGuid().ToString();
        var secondDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(firstDeveloperUserId);
        await _app.CreateUserAsync(secondDeveloperUserId);

        var firstDeveloper = await _tickets.AddCompanyMemberAsync(company.Id, firstDeveloperUserId, RoleNames.Developer,
            saveChanges: false);
        var secondDeveloper = await _tickets.AddCompanyMemberAsync(company.Id, secondDeveloperUserId, RoleNames.Developer,
            saveChanges: false);
        firstDeveloper.AssignedProjects.Add(project);
        secondDeveloper.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/developer", new
        {
            MemberId = firstDeveloper.Id,
            DeveloperId = secondDeveloper.Id
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedTicket = await _db.Tickets.FirstAsync(x => x.Id == ticket.Id);
        Assert.NotEqual(firstDeveloper.Id, unchangedTicket.AssigneeId);
        Assert.NotEqual(secondDeveloper.Id, unchangedTicket.AssigneeId);
    }

    [Fact]
    public async Task UpdateAssignee_With_Developer_Outside_Project_ReturnsBadRequest_And_DoesNotChangeAssignee()
    {
        var (company, _, ticket, admin, _, _, _) = await _tickets.SetupTestScenarioAsync();

        var outsideDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(outsideDeveloperUserId);
        var outsideDeveloper = await _tickets.AddCompanyMemberAsync(company.Id, outsideDeveloperUserId,
            RoleNames.Developer);

        var client = _app.CreateClient(admin.UserId);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/developer", new
        {
            DeveloperId = outsideDeveloper.Id
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        _db.ChangeTracker.Clear();
        var unchangedTicket = await _db.Tickets.FirstAsync(x => x.Id == ticket.Id);
        Assert.NotEqual(outsideDeveloper.Id, unchangedTicket.AssigneeId);
    }
}
