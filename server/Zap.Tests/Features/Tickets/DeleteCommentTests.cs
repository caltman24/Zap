namespace Zap.Tests.Features.Tickets;

public sealed class DeleteCommentTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task Delete_Comment_As_Owner_Returns_204_NoContent_And_Removes_Comment()
    {
        var (_, _, ticket, _, _, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var comment = new TicketComment
        {
            Id = Guid.NewGuid().ToString(),
            TicketId = ticket.Id,
            SenderId = submitter.Id,
            Message = "Test message"
        };

        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}/comments/{comment.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var deletedComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.Null(deletedComment);
    }

    [Fact]
    public async Task Delete_Comment_As_Non_Owner_Returns_NotFound()
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
            Message = "Test message"
        };

        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherUserId);
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}/comments/{comment.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var existingComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.NotNull(existingComment);
    }
}
