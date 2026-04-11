using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Projects;

public sealed class ProjectAuthorizationServiceTests
{
    [Fact]
    public void GetCapabilities_When_Admin_On_Active_Project_Can_Manage_Everything()
    {
        using var db = UnitTestFactory.CreateDbContext();
        var service = new ProjectAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Admin, memberId: "admin-1");
        var project = UnitTestFactory.CreateProject("pm-1");

        var capabilities = service.GetCapabilities(project, currentUser);

        Assert.True(capabilities.CanEdit);
        Assert.True(capabilities.CanArchive);
        Assert.True(capabilities.CanAssignProjectManager);
        Assert.True(capabilities.CanManageMembers);
        Assert.True(capabilities.CanCreateTicket);
    }

    [Fact]
    public void GetCapabilities_When_Submitter_Assigned_To_Active_Project_Can_Create_Ticket_Only()
    {
        using var db = UnitTestFactory.CreateDbContext();
        var service = new ProjectAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Submitter, memberId: "submitter-1");
        var project = UnitTestFactory.CreateProject("pm-1", [new MemberInfoDto("submitter-1", "Submitter User", "avatar", RoleNames.Submitter)]);

        var capabilities = service.GetCapabilities(project, currentUser);

        Assert.False(capabilities.CanEdit);
        Assert.False(capabilities.CanArchive);
        Assert.False(capabilities.CanAssignProjectManager);
        Assert.False(capabilities.CanManageMembers);
        Assert.True(capabilities.CanCreateTicket);
    }

    [Fact]
    public void GetCapabilities_When_Project_Is_Archived_Disables_Manage_And_CreateTicket()
    {
        using var db = UnitTestFactory.CreateDbContext();
        var service = new ProjectAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Admin, memberId: "admin-1");
        var project = UnitTestFactory.CreateProject("pm-1", isArchived: true);

        var capabilities = service.GetCapabilities(project, currentUser);

        Assert.True(capabilities.CanEdit);
        Assert.True(capabilities.CanArchive);
        Assert.False(capabilities.CanAssignProjectManager);
        Assert.False(capabilities.CanManageMembers);
        Assert.False(capabilities.CanCreateTicket);
    }

    [Fact]
    public async Task CanReadProjectAsync_When_Developer_Is_Assigned_Returns_True()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, scenario.Developer.Id, scenario.Company.Id,
            scenario.Developer.UserId);

        var canRead = await service.CanReadProjectAsync(scenario.Project.Id, currentUser);

        Assert.True(canRead);
    }

    [Fact]
    public async Task CanReadProjectAsync_When_Developer_Is_Not_Assigned_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, scenario.OtherDeveloper.Id,
            scenario.Company.Id, scenario.OtherDeveloper.UserId);

        var canRead = await service.CanReadProjectAsync(scenario.Project.Id, currentUser);

        Assert.False(canRead);
    }

    [Fact]
    public async Task CanReadProjectAsync_When_User_Is_From_Another_Company_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectAuthorizationService(db);
        var currentUser = UnitTestFactory.CreateCurrentUser(RoleNames.Developer, scenario.ExternalDeveloper.Id,
            scenario.OtherCompany.Id, scenario.ExternalDeveloper.UserId);

        var canRead = await service.CanReadProjectAsync(scenario.Project.Id, currentUser);

        Assert.False(canRead);
    }
}
