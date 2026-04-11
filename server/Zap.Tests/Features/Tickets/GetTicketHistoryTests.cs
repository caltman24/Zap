namespace Zap.Tests.Features.Tickets;

public sealed class GetTicketHistoryTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task GetTicketHistory_Returns_Ordered_History_With_Formatted_Messages()
    {
        var (_, _, ticket, admin, pm, _, _) = await _tickets.SetupTestScenarioAsync();
        var now = DateTime.UtcNow;

        await _tickets.AddHistoryAsync(ticket.Id, admin.Id, TicketHistoryTypes.UpdatePriority, now.AddMinutes(-2),
            "Low", "High");
        await _tickets.AddHistoryAsync(ticket.Id, pm.Id, TicketHistoryTypes.Resolved, now.AddMinutes(-1));

        var client = _app.CreateClient(admin.UserId);
        var response = await client.GetFromJsonAsync<List<TicketHistoryDto>>($"/tickets/{ticket.Id}/history");

        Assert.NotNull(response);
        Assert.Equal(2, response!.Count);
        Assert.Equal(TicketHistoryTypes.UpdatePriority, response[0].Type);
        Assert.Equal(TicketHistoryTypes.Resolved, response[1].Type);
        Assert.Equal(response.OrderBy(history => history.CreatedAt).Select(history => history.Id),
            response.Select(history => history.Id));
        Assert.Contains("Priority updated from 'Low' to 'High' by John Doe", response[0].FormattedMessage);
        Assert.Contains("Marked as resolved by John Doe", response[1].FormattedMessage);
    }
}
