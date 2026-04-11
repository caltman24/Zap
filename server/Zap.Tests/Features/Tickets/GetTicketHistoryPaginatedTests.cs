namespace Zap.Tests.Features.Tickets;

public sealed class GetTicketHistoryPaginatedTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task GetTicketHistoryPaginated_Returns_Page_Metadata_And_Items()
    {
        var (_, _, ticket, admin, pm, developer, _) = await _tickets.SetupTestScenarioAsync();
        var now = DateTime.UtcNow;

        await _tickets.AddHistoryAsync(ticket.Id, admin.Id, TicketHistoryTypes.UpdatePriority, now.AddMinutes(-3),
            "Low", "High");
        await _tickets.AddHistoryAsync(ticket.Id, pm.Id, TicketHistoryTypes.UpdateStatus, now.AddMinutes(-2),
            TicketStatuses.New, TicketStatuses.Testing);
        await _tickets.AddHistoryAsync(ticket.Id, developer.Id, TicketHistoryTypes.Resolved, now.AddMinutes(-1));

        var client = _app.CreateClient(admin.UserId);
        var response = await client.GetFromJsonAsync<PaginatedResponse<TicketHistoryDto>>(
            $"/tickets/{ticket.Id}/history-pag?page=2&pageSize=1");

        Assert.NotNull(response);
        Assert.Equal(2, response!.Page);
        Assert.Equal(1, response.PageSize);
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(3, response.TotalPages);
        Assert.True(response.HasNextPage);
        Assert.Single(response.Items);
        Assert.Equal(TicketHistoryTypes.UpdateStatus, response.Items[0].Type);
    }
}
