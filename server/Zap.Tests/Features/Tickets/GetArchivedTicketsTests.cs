namespace Zap.Tests.Features.Tickets;

public sealed class GetArchivedTicketsTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task GetArchivedTickets_AsDeveloper_Returns_Archived_Tickets_In_Assigned_Project_Only()
    {
        var (company, project, ticket, _, _, developer, submitter) = await _tickets.SetupTestScenarioAsync();
        ticket.IsArchived = true;

        var visibleArchivedTicket = await _tickets.CreateTicketAsync(project.Id, submitter.Id, developer.Id,
            "Visible Archived Ticket", true);

        var hiddenProject = await _tickets.CreateProjectAsync(company.Id, null, "Hidden Project");
        var hiddenArchivedTicket = await _tickets.CreateTicketAsync(hiddenProject.Id, submitter.Id, null,
            "Hidden Archived Ticket", true);

        await _db.SaveChangesAsync();

        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);
        var response = await client.GetFromJsonAsync<List<BasicTicketDto>>("/tickets/archived");

        Assert.NotNull(response);
        Assert.Contains(response!, current => current.Id == ticket.Id);
        Assert.Contains(response, current => current.Id == visibleArchivedTicket.Id);
        Assert.DoesNotContain(response, current => current.Id == hiddenArchivedTicket.Id);
    }
}
