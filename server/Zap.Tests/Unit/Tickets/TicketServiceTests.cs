using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Tickets;

public sealed class TicketServiceTests
{
    [Fact]
    public async Task CreateTicketAsync_When_Request_Is_Valid_Persists_Ticket_And_Creates_History()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var result = await service.CreateTicketAsync(new CreateTicketDto(
            "Created Ticket",
            "Created Description",
            "low",
            "new",
            "defect",
            scenario.Project.Id,
            scenario.Submitter.Id), scenario.Admin.Id);

        var createdTicket = await db.Tickets.SingleAsync(x => x.Id == result.Id);
        Assert.Equal("Created Ticket", createdTicket.Name);
        Assert.Equal(scenario.Submitter.Id, createdTicket.SubmitterId);
        Assert.Single(history.Entries);
        Assert.Equal(TicketHistoryTypes.Created, history.Entries[0].Type);
    }

    [Fact]
    public async Task UpdateStatusAsync_When_Status_Changes_To_Resolved_Creates_Resolved_History()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        db.TicketStatuses.Add(new TicketStatus { Id = "status-resolved", Name = TicketStatuses.Resolved });
        await db.SaveChangesAsync();
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var updated = await service.UpdateStatusAsync(scenario.Ticket.Id, TicketStatuses.Resolved, scenario.Admin.Id);

        Assert.True(updated);
        Assert.Single(history.Entries);
        Assert.Equal(TicketHistoryTypes.Resolved, history.Entries[0].Type);
        Assert.Equal(scenario.Admin.Id, history.Entries[0].CreatorId);
    }

    [Fact]
    public async Task UpdateStatusAsync_When_Status_Does_Not_Change_Does_Not_Create_History()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var updated = await service.UpdateStatusAsync(scenario.Ticket.Id, TicketStatuses.New, scenario.Admin.Id);

        Assert.True(updated);
        Assert.Empty(history.Entries);
    }

    [Fact]
    public async Task UpdatePriorityAsync_When_Priority_Changes_Creates_Priority_History()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        db.TicketPriorities.Add(new TicketPriority { Id = "priority-high", Name = Priorities.High });
        await db.SaveChangesAsync();
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var updated = await service.UpdatePriorityAsync(scenario.Ticket.Id, Priorities.High, scenario.Admin.Id);

        Assert.True(updated);
        Assert.Single(history.Entries);
        Assert.Equal(TicketHistoryTypes.UpdatePriority, history.Entries[0].Type);
        Assert.Equal(Priorities.Low, history.Entries[0].OldValue);
        Assert.Equal(Priorities.High, history.Entries[0].NewValue);
    }

    [Fact]
    public async Task UpdateTypeAsync_When_Type_Changes_Creates_Type_History()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        db.TicketTypes.Add(new TicketType { Id = "type-feature", Name = TicketTypes.Feature });
        await db.SaveChangesAsync();
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var updated = await service.UpdateTypeAsync(scenario.Ticket.Id, TicketTypes.Feature, scenario.Admin.Id);

        Assert.True(updated);
        Assert.Single(history.Entries);
        Assert.Equal(TicketHistoryTypes.UpdateType, history.Entries[0].Type);
        Assert.Equal(TicketTypes.Defect, history.Entries[0].OldValue);
        Assert.Equal(TicketTypes.Feature, history.Entries[0].NewValue);
    }

    [Fact]
    public async Task UpdateAsigneeAsync_When_Assigning_Developer_Creates_Assigned_History()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var updated = await service.UpdateAsigneeAsync(scenario.Ticket.Id, scenario.OtherDeveloper.Id, scenario.Admin.Id);

        Assert.True(updated);
        var persisted = await db.Tickets.SingleAsync(x => x.Id == scenario.Ticket.Id);
        Assert.Equal(scenario.OtherDeveloper.Id, persisted.AssigneeId);
        Assert.Single(history.Entries);
        Assert.Equal(TicketHistoryTypes.DeveloperAssigned, history.Entries[0].Type);
        Assert.Equal("Other Dev", history.Entries[0].RelatedEntityName);
        Assert.Equal(scenario.OtherDeveloper.Id, history.Entries[0].RelatedEntityId);
    }

    [Fact]
    public async Task UpdateAsigneeAsync_When_Removing_Developer_Creates_Removed_History()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var updated = await service.UpdateAsigneeAsync(scenario.Ticket.Id, null, scenario.Admin.Id);

        Assert.True(updated);
        var persisted = await db.Tickets.SingleAsync(x => x.Id == scenario.Ticket.Id);
        Assert.Null(persisted.AssigneeId);
        Assert.Single(history.Entries);
        Assert.Equal(TicketHistoryTypes.DeveloperRemoved, history.Entries[0].Type);
        Assert.Equal("Dev User", history.Entries[0].RelatedEntityName);
    }

    [Fact]
    public async Task ToggleArchiveTicket_When_Archiving_Creates_Archived_History()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var updated = await service.ToggleArchiveTicket(scenario.Ticket.Id, scenario.Admin.Id);

        Assert.True(updated);
        Assert.Single(history.Entries);
        Assert.Equal(TicketHistoryTypes.Archived, history.Entries[0].Type);
        Assert.True((await db.Tickets.SingleAsync(x => x.Id == scenario.Ticket.Id)).IsArchived);
    }

    [Fact]
    public async Task UpdateArchivedTicketAsync_When_Name_And_Description_Change_Creates_History_For_Both()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        scenario.Ticket.IsArchived = true;
        await db.SaveChangesAsync();
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var updated = await service.UpdateArchivedTicketAsync(scenario.Ticket.Id, "Renamed", "Updated description",
            scenario.Admin.Id);

        Assert.True(updated);
        Assert.Equal(2, history.Entries.Count);
        Assert.Contains(history.Entries, entry => entry.Type == TicketHistoryTypes.UpdateName && entry.OldValue == "Ticket" && entry.NewValue == "Renamed");
        Assert.Contains(history.Entries, entry => entry.Type == TicketHistoryTypes.UpdateDescription);
    }

    [Fact]
    public async Task UpdateTicketAsync_When_Multiple_Fields_Change_Creates_History_Per_Field()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        db.TicketPriorities.Add(new TicketPriority { Id = "priority-high", Name = Priorities.High });
        db.TicketStatuses.Add(new TicketStatus { Id = "status-testing", Name = TicketStatuses.Testing });
        db.TicketTypes.Add(new TicketType { Id = "type-feature", Name = TicketTypes.Feature });
        await db.SaveChangesAsync();
        var history = new RecordingTicketHistoryService();
        var service = new TicketService(db, history);

        var updated = await service.UpdateTicketAsync(scenario.Ticket.Id,
            new UpdateTicketDto("Renamed Ticket", "New Description", Priorities.High, TicketStatuses.Testing,
                TicketTypes.Feature), scenario.Admin.Id);

        Assert.True(updated);
        Assert.Equal(5, history.Entries.Count);
        Assert.Contains(history.Entries, entry => entry.Type == TicketHistoryTypes.UpdateName);
        Assert.Contains(history.Entries, entry => entry.Type == TicketHistoryTypes.UpdateDescription);
        Assert.Contains(history.Entries, entry => entry.Type == TicketHistoryTypes.UpdatePriority);
        Assert.Contains(history.Entries, entry => entry.Type == TicketHistoryTypes.UpdateStatus);
        Assert.Contains(history.Entries, entry => entry.Type == TicketHistoryTypes.UpdateType);
    }

    private sealed class RecordingTicketHistoryService : ITicketHistoryService
    {
        public List<RecordedHistoryEntry> Entries { get; } = [];

        public Task CreateHistoryEntryAsync(string ticketId, string creatorId, TicketHistoryTypes type, string? oldValue = null,
            string? newValue = null, string? relatedEntityName = null, string? relatedEntityId = null)
        {
            Entries.Add(new RecordedHistoryEntry(ticketId, creatorId, type, oldValue, newValue, relatedEntityName,
                relatedEntityId));
            return Task.CompletedTask;
        }

        public Task<List<TicketHistoryDto>> GetTicketHistoryAsync(string ticketId)
        {
            throw new NotSupportedException();
        }

        public Task<PaginatedResponse<TicketHistoryDto>> GetTicketHistoryAsync(string ticketId, int page, int pageSize)
        {
            throw new NotSupportedException();
        }
    }

    private sealed record RecordedHistoryEntry(
        string TicketId,
        string CreatorId,
        TicketHistoryTypes Type,
        string? OldValue,
        string? NewValue,
        string? RelatedEntityName,
        string? RelatedEntityId);
}
