namespace Zap.Tests.Features.Tickets;

public sealed class UpdateCommentTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task Update_Comment_As_Owner_Returns_204_NoContent_And_Persists_Changes()
    {
        var (_, _, ticket, _, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var comment = new TicketComment
        {
            Id = Guid.NewGuid().ToString(),
            TicketId = ticket.Id,
            SenderId = submitter.Id,
            Message = "Original message"
        };

        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/comments/{comment.Id}",
            new { Message = "Updated message" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.NotNull(updatedComment);
        Assert.Equal("Updated message", updatedComment.Message);
        Assert.NotNull(updatedComment.UpdatedAt);
    }

    [Fact]
    public async Task Update_Comment_As_Non_Owner_Returns_NotFound()
    {
        var (company, project, ticket, _, _, _, submitter) = await _tickets.SetupTestScenarioAsync();

        var otherUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherUserId);
        var otherMember = await _tickets.AddCompanyMemberAsync(company.Id, otherUserId, RoleNames.Developer,
            saveChanges: false);
        otherMember.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var comment = new TicketComment
        {
            Id = Guid.NewGuid().ToString(),
            TicketId = ticket.Id,
            SenderId = submitter.Id,
            Message = "Original message"
        };

        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherUserId);
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/comments/{comment.Id}",
            new { Message = "Updated message" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var unchangedComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.NotNull(unchangedComment);
        Assert.Equal("Original message", unchangedComment.Message);
        Assert.Null(unchangedComment.UpdatedAt);
    }
}
