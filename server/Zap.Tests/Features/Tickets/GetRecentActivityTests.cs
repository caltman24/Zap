namespace Zap.Tests.Features.Tickets;

public sealed class GetRecentActivityTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task GetRecentActivity_AsAdmin_ReturnsLatestFiveNonCommentEvents()
    {
        var (_, project, ticket, admin, pm, developer, submitter) = await _tickets.SetupTestScenarioAsync();
        var now = DateTime.UtcNow;
        var secondTicket = await _tickets.CreateTicketAsync(project.Id, submitter.Id, developer.Id, "Second Ticket");

        await _tickets.AddHistoryAsync(ticket.Id, submitter.Id, TicketHistoryTypes.Created, now.AddMinutes(-6));
        await _tickets.AddHistoryAsync(ticket.Id, pm.Id, TicketHistoryTypes.UpdatePriority, now.AddMinutes(-5), "Low",
            "High");
        await _tickets.AddHistoryAsync(ticket.Id, developer.Id, TicketHistoryTypes.UpdateStatus, now.AddMinutes(-4),
            "New", "Testing");
        await _tickets.AddHistoryAsync(secondTicket.Id, pm.Id, TicketHistoryTypes.DeveloperAssigned, now.AddMinutes(-3),
            relatedEntityName: "Assigned Dev");
        await _tickets.AddHistoryAsync(secondTicket.Id, pm.Id, TicketHistoryTypes.Resolved, now.AddMinutes(-2));
        await _tickets.AddHistoryAsync(secondTicket.Id, pm.Id, TicketHistoryTypes.DeveloperRemoved, now.AddMinutes(-1),
            relatedEntityName: "Assigned Dev");
        await _tickets.AddCommentAsync(ticket.Id, admin.Id, "Hidden admin comment", now);

        var client = _app.CreateClient(admin.UserId);
        var response = await client.GetFromJsonAsync<List<RecentActivityDto>>("/tickets/recent-activity");

        Assert.NotNull(response);
        Assert.Equal(5, response.Count);
        Assert.DoesNotContain(response, activity => activity.Type == RecentActivityTypes.CommentAdded);
        Assert.Equal(response.OrderByDescending(activity => activity.OccurredAt).Select(activity => activity.Id),
            response.Select(activity => activity.Id));
        Assert.Equal(RecentActivityTypes.AssigneeChanged, response[0].Type);
    }

    [Fact]
    public async Task GetRecentActivity_AsDeveloper_ReturnsProjectLifecycleAndAssignedCommentsOnly()
    {
        var (company, project, ticket, _, pm, _, submitter) = await _tickets.SetupTestScenarioAsync();
        var now = DateTime.UtcNow;

        var otherDeveloperUserId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(otherDeveloperUserId);
        var otherDeveloper = await _tickets.AddCompanyMemberAsync(company.Id, otherDeveloperUserId, RoleNames.Developer,
            saveChanges: false);
        otherDeveloper.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var otherTicket =
            await _tickets.CreateTicketAsync(project.Id, submitter.Id, otherDeveloper.Id, "Project Ticket");

        await _tickets.AddHistoryAsync(otherTicket.Id, pm.Id, TicketHistoryTypes.UpdatePriority, now.AddMinutes(-2),
            "Low", "Urgent");
        await _tickets.AddCommentAsync(otherTicket.Id, pm.Id, "Not assigned comment", now.AddMinutes(-1));
        await _tickets.AddCommentAsync(ticket.Id, pm.Id, "Assigned ticket comment", now);

        var client =
            _app.CreateClient((await _db.CompanyMembers.FirstAsync(member => member.Id == ticket.AssigneeId)).UserId,
                RoleNames.Developer);
        var response = await client.GetFromJsonAsync<List<RecentActivityDto>>("/tickets/recent-activity");

        Assert.NotNull(response);
        Assert.Contains(response,
            activity => activity.TicketId == otherTicket.Id && activity.Type == RecentActivityTypes.PriorityChanged);
        Assert.DoesNotContain(response,
            activity => activity.TicketId == otherTicket.Id && activity.Type == RecentActivityTypes.CommentAdded);
        Assert.Contains(response, activity =>
            activity.TicketId == ticket.Id && activity.Type == RecentActivityTypes.CommentAdded &&
            activity.Message == "Assigned ticket comment");
    }

    [Fact]
    public async Task GetRecentActivity_AsSubmitter_ReturnsProjectLifecycleAndOwnTicketCommentsOnly()
    {
        var (_, project, ticket, _, pm, developer, submitter) = await _tickets.SetupTestScenarioAsync();
        var now = DateTime.UtcNow;

        submitter.AssignedProjects.Add(project);
        await _db.SaveChangesAsync();

        var otherTicket =
            await _tickets.CreateTicketAsync(project.Id, developer.Id, developer.Id, "Shared Project Ticket");

        await _tickets.AddHistoryAsync(otherTicket.Id, pm.Id, TicketHistoryTypes.UpdateStatus, now.AddMinutes(-2),
            "New", "In Development");
        await _tickets.AddCommentAsync(otherTicket.Id, pm.Id, "Project comment", now.AddMinutes(-1));
        await _tickets.AddCommentAsync(ticket.Id, pm.Id, "Own ticket comment", now);

        var client = _app.CreateClient(submitter.UserId, RoleNames.Submitter);
        var response = await client.GetFromJsonAsync<List<RecentActivityDto>>("/tickets/recent-activity");

        Assert.NotNull(response);
        Assert.Contains(response,
            activity => activity.TicketId == otherTicket.Id && activity.Type == RecentActivityTypes.StatusChanged);
        Assert.DoesNotContain(response,
            activity => activity.TicketId == otherTicket.Id && activity.Type == RecentActivityTypes.CommentAdded);
        Assert.Contains(response, activity =>
            activity.TicketId == ticket.Id && activity.Type == RecentActivityTypes.CommentAdded &&
            activity.Message == "Own ticket comment");
    }
}