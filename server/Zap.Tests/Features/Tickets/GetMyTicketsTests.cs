namespace Zap.Tests.Features.Tickets;

public sealed class GetMyTicketsTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task GetMyTickets_AsDeveloper_Returns_Assigned_And_Submitted_Tickets_Only()
    {
        var (_, project, assignedTicket, _, _, developer, submitter) = await _tickets.SetupTestScenarioAsync();
        var submittedTicket = await _tickets.CreateTicketAsync(project.Id, developer.Id, developer.Id, "Submitted Ticket");
        var hiddenTicket = await _tickets.CreateTicketAsync(project.Id, submitter.Id, null, "Hidden Ticket");

        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);
        var response = await client.GetFromJsonAsync<List<BasicTicketDto>>("/tickets/mytickets");

        Assert.NotNull(response);
        Assert.Contains(response!, ticket => ticket.Id == assignedTicket.Id);
        Assert.Contains(response, ticket => ticket.Id == submittedTicket.Id);
        Assert.DoesNotContain(response, ticket => ticket.Id == hiddenTicket.Id);
    }
}
