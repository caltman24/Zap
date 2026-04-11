namespace Zap.Tests.Features.Tickets;

public sealed class DeleteCommentTests : IntegrationTestBase
{
    [Fact]
    public async Task Delete_Comment_As_Owner_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CompanyTestData.CreateTestCompanyAsync(_db, userId, user);
        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Project",
            Description = "Test Description",
            Priority = "High",
            CompanyId = company.Id,
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        var ticket = new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Ticket",
            Description = "Test Description",
            ProjectId = project.Id,
            SubmitterId = company.Members.First().Id,
            PriorityId = (await _db.TicketPriorities.FirstAsync()).Id,
            StatusId = (await _db.TicketStatuses.FirstAsync()).Id,
            TypeId = (await _db.TicketTypes.FirstAsync()).Id
        };

        var comment = new TicketComment
        {
            Id = Guid.NewGuid().ToString(),
            TicketId = ticket.Id,
            SenderId = company.Members.First().Id,
            Message = "Test message"
        };

        _db.Projects.Add(project);
        _db.Tickets.Add(ticket);
        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(userId);
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}/comments/{comment.Id}");

        Assert.True(response.IsSuccessStatusCode);

        _db.ChangeTracker.Clear();
        var deletedComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.Null(deletedComment);
    }

    [Fact]
    public async Task Delete_Comment_As_Non_Owner_Returns_NotFound()
    {
        var ownerUserId = Guid.NewGuid().ToString();
        var otherUserId = Guid.NewGuid().ToString();

        await _app.CreateUserAsync(ownerUserId);
        await _app.CreateUserAsync(otherUserId);

        var ownerUser = await _db.Users.FindAsync(ownerUserId);
        var otherUser = await _db.Users.FindAsync(otherUserId);
        Assert.NotNull(ownerUser);
        Assert.NotNull(otherUser);

        var company = await CompanyTestData.CreateTestCompanyAsync(_db, ownerUserId, ownerUser);

        var otherMember = new CompanyMember
        {
            UserId = otherUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(role => role.Name == RoleNames.Developer).Select(role => role.Id)
                .FirstAsync()
        };
        _db.CompanyMembers.Add(otherMember);
        await _db.SaveChangesAsync();

        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Project",
            Description = "Test Description",
            Priority = "High",
            CompanyId = company.Id,
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        var ticket = new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Ticket",
            Description = "Test Description",
            ProjectId = project.Id,
            SubmitterId = company.Members.First().Id,
            AssigneeId = otherMember.Id,
            PriorityId = (await _db.TicketPriorities.FirstAsync()).Id,
            StatusId = (await _db.TicketStatuses.FirstAsync()).Id,
            TypeId = (await _db.TicketTypes.FirstAsync()).Id
        };

        otherMember.AssignedProjects.Add(project);

        var comment = new TicketComment
        {
            Id = Guid.NewGuid().ToString(),
            TicketId = ticket.Id,
            SenderId = company.Members.First().Id,
            Message = "Test message"
        };

        _db.Projects.Add(project);
        _db.Tickets.Add(ticket);
        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherUserId);
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}/comments/{comment.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var existingComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.NotNull(existingComment);
    }
}