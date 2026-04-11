namespace Zap.Tests.Features.Tickets;

public sealed class GetResolvedTicketsTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task GetResolvedTickets_AsAdmin_Returns_Resolved_Unarchived_Tickets_Only()
    {
        var (_, project, ticket, admin, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var resolvedStatusId = await _db.TicketStatuses
            .Where(status => status.Name == TicketStatuses.Resolved)
            .Select(status => status.Id)
            .SingleAsync();

        ticket.StatusId = resolvedStatusId;

        var openTicket = await _tickets.CreateTicketAsync(project.Id, submitter.Id, name: "Open Ticket");
        var archivedTicket = await _tickets.CreateTicketAsync(project.Id, submitter.Id, name: "Archived Ticket",
            isArchived: true);
        archivedTicket.StatusId = resolvedStatusId;
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(admin.UserId);
        var response = await client.GetFromJsonAsync<List<BasicTicketDto>>("/tickets/resolved");

        Assert.NotNull(response);
        Assert.Contains(response!, current => current.Id == ticket.Id);
        Assert.DoesNotContain(response, current => current.Id == openTicket.Id);
        Assert.DoesNotContain(response, current => current.Id == archivedTicket.Id);
    }
}
