namespace Zap.Tests.Features.Tickets;

public sealed class GetOpenTicketsTests : TicketIntegrationTestBase
{
    [Fact]
    public async Task GetOpenTickets_AsDeveloperAssignedToProject_ReturnsProjectTicketsOnly()
    {
        var (company, project, ticket, _, _, developer, submitter) = await _tickets.SetupTestScenarioAsync();

        var otherProject = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Other Project",
            Description = "Other Project Description",
            Priority = "High",
            CompanyId = company.Id,
            DueDate = DateTime.UtcNow.AddDays(7),
            IsArchived = false
        };
        _db.Projects.Add(otherProject);

        _db.Tickets.Add(new Ticket
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Other Ticket",
            Description = "Other Description",
            ProjectId = otherProject.Id,
            SubmitterId = submitter.Id,
            PriorityId = await _db.TicketPriorities.Where(priority => priority.Name == Priorities.Low)
                .Select(priority => priority.Id).FirstAsync(),
            StatusId = await _db.TicketStatuses.Where(status => status.Name == TicketStatuses.New)
                .Select(status => status.Id).FirstAsync(),
            TypeId = await _db.TicketTypes.Where(type => type.Name == TicketTypes.Defect).Select(type => type.Id)
                .FirstAsync(),
            IsArchived = false
        });
        await _db.SaveChangesAsync();

        var client = _app.CreateClient(developer.UserId, RoleNames.Developer);
        var response = await client.GetFromJsonAsync<List<BasicTicketDto>>("/tickets/open");

        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal(ticket.Id, response[0].Id);
        Assert.Equal(project.Id, response[0].ProjectId);
    }
}