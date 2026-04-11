using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Tickets;

public sealed class TicketAuthorizationServiceTests
{
    [Fact]
    public void GetCapabilities_When_Admin_On_Active_Ticket_Can_Manage_Everything()
    {
        using var db = UnitTestFactory.CreateDbContext();
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Admin, memberId: "admin-1");
        var ticket = UnitTestFactory.CreateBasicTicket(roleOwnerId: "submitter-1", assigneeId: "dev-1", projectManagerId: "pm-1");

        var capabilities = service.GetCapabilities(ticket, currentUser);

        Assert.True(capabilities.CanEditDetails);
        Assert.True(capabilities.CanEditNameDescription);
        Assert.True(capabilities.CanUpdatePriority);
        Assert.True(capabilities.CanUpdateStatus);
        Assert.True(capabilities.CanUpdateType);
        Assert.True(capabilities.CanAssignDeveloper);
        Assert.True(capabilities.CanArchive);
        Assert.True(capabilities.CanUnarchive);
        Assert.True(capabilities.CanDelete);
        Assert.True(capabilities.CanComment);
    }

    [Fact]
    public void GetCapabilities_When_Assigned_Developer_Can_Update_Status_And_Comment_Only()
    {
        using var db = UnitTestFactory.CreateDbContext();
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, memberId: "dev-1");
        var ticket = UnitTestFactory.CreateBasicTicket(roleOwnerId: "submitter-1", assigneeId: "dev-1", projectManagerId: "pm-1");

        var capabilities = service.GetCapabilities(ticket, currentUser);

        Assert.True(capabilities.CanEditDetails);
        Assert.False(capabilities.CanEditNameDescription);
        Assert.False(capabilities.CanUpdatePriority);
        Assert.True(capabilities.CanUpdateStatus);
        Assert.False(capabilities.CanUpdateType);
        Assert.False(capabilities.CanAssignDeveloper);
        Assert.False(capabilities.CanArchive);
        Assert.False(capabilities.CanDelete);
        Assert.True(capabilities.CanComment);
    }

    [Fact]
    public void GetCapabilities_When_Submitter_On_New_Ticket_Can_Edit_Name_And_Description_And_Comment()
    {
        using var db = UnitTestFactory.CreateDbContext();
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, memberId: "submitter-1");
        var ticket = UnitTestFactory.CreateBasicTicket(roleOwnerId: "submitter-1", assigneeId: "dev-1", projectManagerId: "pm-1", submitterId: "submitter-1");

        var capabilities = service.GetCapabilities(ticket, currentUser);

        Assert.True(capabilities.CanEditDetails);
        Assert.True(capabilities.CanEditNameDescription);
        Assert.False(capabilities.CanUpdatePriority);
        Assert.False(capabilities.CanUpdateStatus);
        Assert.False(capabilities.CanDelete);
        Assert.True(capabilities.CanComment);
    }

    [Fact]
    public void GetCapabilities_When_Ticket_Is_Archived_Disables_Mutation_Capabilities()
    {
        using var db = UnitTestFactory.CreateDbContext();
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Admin, memberId: "admin-1");
        var ticket = UnitTestFactory.CreateBasicTicket(roleOwnerId: "submitter-1", assigneeId: "dev-1", projectManagerId: "pm-1", isArchived: true);

        var capabilities = service.GetCapabilities(ticket, currentUser);

        Assert.True(capabilities.CanEditDetails);
        Assert.True(capabilities.CanEditNameDescription);
        Assert.False(capabilities.CanUpdatePriority);
        Assert.False(capabilities.CanUpdateStatus);
        Assert.False(capabilities.CanUpdateType);
        Assert.False(capabilities.CanAssignDeveloper);
        Assert.True(capabilities.CanArchive);
        Assert.True(capabilities.CanUnarchive);
        Assert.False(capabilities.CanDelete);
        Assert.True(capabilities.CanComment);
    }

    [Fact]
    public async Task CanCreateTicketInProjectAsync_When_Submitter_Is_Assigned_Returns_True()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, scenario.Submitter.Id,
            scenario.Company.Id, scenario.Submitter.UserId);

        var canCreate = await service.CanCreateTicketInProjectAsync(scenario.Project.Id, currentUser);

        Assert.True(canCreate);
    }

    [Fact]
    public async Task CanCreateTicketInProjectAsync_When_Submitter_Is_Not_Assigned_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, "submitter-2",
            scenario.Company.Id, "submitter-user-2");

        var canCreate = await service.CanCreateTicketInProjectAsync(scenario.Project.Id, currentUser);

        Assert.False(canCreate);
    }

    [Fact]
    public async Task CanUpdateStatusAsync_When_Developer_Is_Assigned_Returns_True()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, scenario.Developer.Id,
            scenario.Company.Id, scenario.Developer.UserId);

        var canUpdate = await service.CanUpdateStatusAsync(scenario.Ticket.Id, currentUser);

        Assert.True(canUpdate);
    }

    [Fact]
    public async Task CanUpdateStatusAsync_When_Developer_Is_Not_Assigned_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, scenario.OtherDeveloper.Id,
            scenario.Company.Id, scenario.OtherDeveloper.UserId);

        var canUpdate = await service.CanUpdateStatusAsync(scenario.Ticket.Id, currentUser);

        Assert.False(canUpdate);
    }

    [Fact]
    public async Task CanCommentTicketAsync_When_Submitter_Owns_Ticket_Returns_True()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, scenario.Submitter.Id,
            scenario.Company.Id, scenario.Submitter.UserId);

        var canComment = await service.CanCommentTicketAsync(scenario.Ticket.Id, currentUser);

        Assert.True(canComment);
    }

    [Fact]
    public async Task CanCommentTicketAsync_When_User_Is_From_Another_Company_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, scenario.ExternalDeveloper.Id,
            scenario.OtherCompany.Id, scenario.ExternalDeveloper.UserId);

        var canComment = await service.CanCommentTicketAsync(scenario.Ticket.Id, currentUser);

        Assert.False(canComment);
    }

    [Fact]
    public async Task CanEditTicketDetailsAsync_When_Submitter_Ticket_Is_New_Returns_True()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, scenario.Submitter.Id,
            scenario.Company.Id, scenario.Submitter.UserId);

        var canEdit = await service.CanEditTicketDetailsAsync(scenario.Ticket.Id, currentUser);

        Assert.True(canEdit);
    }

    [Fact]
    public async Task CanEditTicketDetailsAsync_When_Submitter_Ticket_Is_Not_New_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        scenario.Ticket.Status = new TicketStatus { Id = "status-testing", Name = TicketStatuses.Testing };
        scenario.Ticket.StatusId = scenario.Ticket.Status.Id;
        db.TicketStatuses.Add(scenario.Ticket.Status);
        await db.SaveChangesAsync();
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, scenario.Submitter.Id,
            scenario.Company.Id, scenario.Submitter.UserId);

        var canEdit = await service.CanEditTicketDetailsAsync(scenario.Ticket.Id, currentUser);

        Assert.False(canEdit);
    }

    [Fact]
    public void GetCapabilities_When_Submitter_Ticket_Is_Archived_Cannot_Edit_Name_Description()
    {
        using var db = UnitTestFactory.CreateDbContext();
        var service = new TicketAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, memberId: "submitter-1");
        var ticket = UnitTestFactory.CreateBasicTicket(roleOwnerId: "submitter-1", assigneeId: "dev-1",
            projectManagerId: "pm-1", isArchived: true, submitterId: "submitter-1");

        var capabilities = service.GetCapabilities(ticket, currentUser);

        Assert.False(capabilities.CanEditNameDescription);
        Assert.False(capabilities.CanEditDetails);
        Assert.True(capabilities.CanComment);
    }
}
