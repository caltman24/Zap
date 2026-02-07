namespace Zap.Tests.IntegrationTests;

public class CommentTests : IAsyncDisposable
{
    private readonly ZapApplication _app;
    private readonly AppDbContext _db;

    public CommentTests()
    {
        _app = new ZapApplication();
        _db = _app.CreateAppDbContext();
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task Update_Comment_As_Owner_Returns_Success()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CompaniesTests.CreateTestCompany(_db, userId, user);
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
            Message = "Original message"
        };

        _db.Projects.Add(project);
        _db.Tickets.Add(ticket);
        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(userId);
        var updateRequest = new { Message = "Updated message" };

        // Act
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/comments/{comment.Id}", updateRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        _db.ChangeTracker.Clear();
        var updatedComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.NotNull(updatedComment);
        Assert.Equal("Updated message", updatedComment.Message);
        Assert.NotNull(updatedComment.UpdatedAt);
    }

    [Fact]
    public async Task Update_Comment_As_Non_Owner_Returns_NotFound()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid().ToString();
        var otherUserId = Guid.NewGuid().ToString();

        await _app.CreateUserAsync(ownerUserId);
        await _app.CreateUserAsync(otherUserId);

        var ownerUser = await _db.Users.FindAsync(ownerUserId);
        var otherUser = await _db.Users.FindAsync(otherUserId);
        Assert.NotNull(ownerUser);
        Assert.NotNull(otherUser);

        var company = await CompaniesTests.CreateTestCompany(_db, ownerUserId, ownerUser);

        // Add other user to the same company
        var otherMember = new CompanyMember
        {
            UserId = otherUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == "Developer").Select(r => r.Id).FirstAsync()
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
            AssigneeId = otherMember.Id, // Assign otherMember so they pass the ticket company validation filter
            PriorityId = (await _db.TicketPriorities.FirstAsync()).Id,
            StatusId = (await _db.TicketStatuses.FirstAsync()).Id,
            TypeId = (await _db.TicketTypes.FirstAsync()).Id
        };

        var comment = new TicketComment
        {
            Id = Guid.NewGuid().ToString(),
            TicketId = ticket.Id,
            SenderId = company.Members.First().Id, // Owner's comment
            Message = "Original message"
        };

        _db.Projects.Add(project);
        _db.Tickets.Add(ticket);
        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherUserId); // Different user trying to edit
        var updateRequest = new { Message = "Updated message" };

        // Act
        var response = await client.PutAsJsonAsync($"/tickets/{ticket.Id}/comments/{comment.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var unchangedComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.NotNull(unchangedComment);
        Assert.Equal("Original message", unchangedComment.Message); // Should remain unchanged
        Assert.Null(unchangedComment.UpdatedAt);
    }

    [Fact]
    public async Task Delete_Comment_As_Owner_Returns_Success()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CompaniesTests.CreateTestCompany(_db, userId, user);
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

        // Act
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}/comments/{comment.Id}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        _db.ChangeTracker.Clear();
        var deletedComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.Null(deletedComment); // Should be deleted
    }

    [Fact]
    public async Task Delete_Comment_As_Non_Owner_Returns_NotFound()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid().ToString();
        var otherUserId = Guid.NewGuid().ToString();

        await _app.CreateUserAsync(ownerUserId);
        await _app.CreateUserAsync(otherUserId);

        var ownerUser = await _db.Users.FindAsync(ownerUserId);
        var otherUser = await _db.Users.FindAsync(otherUserId);
        Assert.NotNull(ownerUser);
        Assert.NotNull(otherUser);

        var company = await CompaniesTests.CreateTestCompany(_db, ownerUserId, ownerUser);

        // Add other user to the same company
        var otherMember = new CompanyMember
        {
            UserId = otherUserId,
            CompanyId = company.Id,
            RoleId = await _db.CompanyRoles.Where(r => r.Name == "Developer").Select(r => r.Id).FirstAsync()
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
            AssigneeId = otherMember.Id, // Assign otherMember so they pass the ticket company validation filter
            PriorityId = (await _db.TicketPriorities.FirstAsync()).Id,
            StatusId = (await _db.TicketStatuses.FirstAsync()).Id,
            TypeId = (await _db.TicketTypes.FirstAsync()).Id
        };

        var comment = new TicketComment
        {
            Id = Guid.NewGuid().ToString(),
            TicketId = ticket.Id,
            SenderId = company.Members.First().Id, // Owner's comment
            Message = "Test message"
        };

        _db.Projects.Add(project);
        _db.Tickets.Add(ticket);
        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(otherUserId); // Different user trying to delete

        // Act
        var response = await client.DeleteAsync($"/tickets/{ticket.Id}/comments/{comment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var existingComment = await _db.TicketComments.FindAsync(comment.Id);
        Assert.NotNull(existingComment); // Should still exist
    }
}