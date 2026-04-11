namespace Zap.Tests.Features.Tickets;

public sealed class GetCommentsTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task GetComments_Returns_Ordered_Comments_With_Capabilities_For_Current_User()
    {
        var (_, _, ticket, _, pm, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var now = DateTime.UtcNow;

        await _tickets.AddCommentAsync(ticket.Id, submitter.Id, "First comment", now.AddMinutes(-1));
        await _tickets.AddCommentAsync(ticket.Id, pm.Id, "Second comment", now);

        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);
        var response = await client.GetFromJsonAsync<List<CommentDto>>($"/tickets/{ticket.Id}/comments");

        Assert.NotNull(response);
        Assert.Equal(["First comment", "Second comment"], response!.Select(comment => comment.Message).ToArray());
        Assert.True(response[0].Capabilities.CanEdit);
        Assert.True(response[0].Capabilities.CanDelete);
        Assert.False(response[1].Capabilities.CanEdit);
        Assert.False(response[1].Capabilities.CanDelete);
    }
}
