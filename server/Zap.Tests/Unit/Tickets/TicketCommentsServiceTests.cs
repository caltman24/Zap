using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Tickets;

public sealed class TicketCommentsServiceTests
{
    [Fact]
    public async Task UpdateCommentAsync_When_Sender_Owns_Comment_Updates_Message_And_Timestamp()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = UnitTestFactory.CreateTicketCommentsService(db);

        var comment = new TicketComment
        {
            Id = "comment-1",
            TicketId = scenario.Ticket.Id,
            Ticket = scenario.Ticket,
            SenderId = scenario.Submitter.Id,
            Sender = scenario.Submitter,
            Message = "Original"
        };
        db.TicketComments.Add(comment);
        await db.SaveChangesAsync();

        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, scenario.Submitter.Id, scenario.Company.Id,
            scenario.Submitter.UserId);
        var updated = await service.UpdateCommentAsync(scenario.Ticket.Id, comment.Id, "Updated", currentUser);

        Assert.True(updated);

        var persisted = await db.TicketComments.SingleAsync(x => x.Id == comment.Id);
        Assert.Equal("Updated", persisted.Message);
        Assert.NotNull(persisted.UpdatedAt);
    }

    [Fact]
    public async Task UpdateCommentAsync_When_CurrentUser_Does_Not_Own_Comment_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = UnitTestFactory.CreateTicketCommentsService(db);

        var comment = new TicketComment
        {
            Id = "comment-1",
            TicketId = scenario.Ticket.Id,
            Ticket = scenario.Ticket,
            SenderId = scenario.Submitter.Id,
            Sender = scenario.Submitter,
            Message = "Original"
        };
        db.TicketComments.Add(comment);
        await db.SaveChangesAsync();

        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, scenario.Developer.Id, scenario.Company.Id,
            scenario.Developer.UserId);
        var updated = await service.UpdateCommentAsync(scenario.Ticket.Id, comment.Id, "Blocked", currentUser);

        Assert.False(updated);

        var persisted = await db.TicketComments.SingleAsync(x => x.Id == comment.Id);
        Assert.Equal("Original", persisted.Message);
        Assert.Null(persisted.UpdatedAt);
    }

    [Fact]
    public async Task DeleteCommentAsync_When_ProjectManager_Deletes_Developer_Comment_Returns_True()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = UnitTestFactory.CreateTicketCommentsService(db);

        var comment = new TicketComment
        {
            Id = "comment-1",
            TicketId = scenario.Ticket.Id,
            Ticket = scenario.Ticket,
            SenderId = scenario.Developer.Id,
            Sender = scenario.Developer,
            Message = "Developer comment"
        };
        db.TicketComments.Add(comment);
        await db.SaveChangesAsync();

        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.ProjectManager, scenario.ProjectManager.Id,
            scenario.Company.Id, scenario.ProjectManager.UserId);
        var deleted = await service.DeleteCommentAsync(scenario.Ticket.Id, comment.Id, currentUser);

        Assert.True(deleted);
        Assert.Null(await db.TicketComments.FindAsync(comment.Id));
    }

    [Fact]
    public async Task DeleteCommentAsync_When_ProjectManager_Deletes_Admin_Comment_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = UnitTestFactory.CreateTicketCommentsService(db);

        var comment = new TicketComment
        {
            Id = "comment-1",
            TicketId = scenario.Ticket.Id,
            Ticket = scenario.Ticket,
            SenderId = scenario.Admin.Id,
            Sender = scenario.Admin,
            Message = "Admin comment"
        };
        db.TicketComments.Add(comment);
        await db.SaveChangesAsync();

        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.ProjectManager, scenario.ProjectManager.Id,
            scenario.Company.Id, scenario.ProjectManager.UserId);
        var deleted = await service.DeleteCommentAsync(scenario.Ticket.Id, comment.Id, currentUser);

        Assert.False(deleted);
        Assert.NotNull(await db.TicketComments.FindAsync(comment.Id));
    }

    [Fact]
    public async Task GetCommentsAsync_When_Viewed_By_Submitter_Sets_Capabilities_Per_Comment()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = UnitTestFactory.CreateTicketCommentsService(db);

        db.TicketComments.AddRange(
            new TicketComment
            {
                Id = "comment-1",
                TicketId = scenario.Ticket.Id,
                Ticket = scenario.Ticket,
                SenderId = scenario.Submitter.Id,
                Sender = scenario.Submitter,
                Message = "My comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            },
            new TicketComment
            {
                Id = "comment-2",
                TicketId = scenario.Ticket.Id,
                Ticket = scenario.Ticket,
                SenderId = scenario.ProjectManager.Id,
                Sender = scenario.ProjectManager,
                Message = "PM comment",
                CreatedAt = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, scenario.Submitter.Id, scenario.Company.Id,
            scenario.Submitter.UserId);
        var comments = await service.GetCommentsAsync(scenario.Ticket.Id, currentUser);

        Assert.Equal(2, comments.Count);
        Assert.Equal(["My comment", "PM comment"], comments.Select(x => x.Message).ToArray());
        Assert.True(comments[0].Capabilities.CanEdit);
        Assert.True(comments[0].Capabilities.CanDelete);
        Assert.False(comments[1].Capabilities.CanEdit);
        Assert.False(comments[1].Capabilities.CanDelete);
    }
}
