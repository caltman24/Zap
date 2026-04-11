using Microsoft.Extensions.Logging.Abstractions;
using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Projects;

public sealed class ProjectServiceTests
{
    [Fact]
    public async Task AreMembersInProjectCompanyAsync_When_All_Members_Belong_To_Project_Company_Returns_True()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectService(db, NullLogger<ProjectService>.Instance, UnitTestFactory.CreateCompanyService(db));

        var result = await service.AreMembersInProjectCompanyAsync(scenario.Project.Id,
            [scenario.Developer.Id, scenario.Submitter.Id]);

        Assert.True(result);
    }

    [Fact]
    public async Task AreMembersInProjectCompanyAsync_When_Any_Member_Is_From_Another_Company_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectService(db, NullLogger<ProjectService>.Instance, UnitTestFactory.CreateCompanyService(db));

        var result = await service.AreMembersInProjectCompanyAsync(scenario.Project.Id,
            [scenario.Developer.Id, scenario.ExternalDeveloper.Id]);

        Assert.False(result);
    }

    [Fact]
    public async Task AreMembersInProjectCompanyAsync_When_Input_Is_Empty_Returns_False()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectService(db, NullLogger<ProjectService>.Instance, UnitTestFactory.CreateCompanyService(db));

        var result = await service.AreMembersInProjectCompanyAsync(scenario.Project.Id, []);

        Assert.False(result);
    }

    [Fact]
    public async Task GetUnassignedMembersAsync_Returns_Only_Unassigned_Submitters_And_Developers_Excluding_Current_Member()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectService(db, NullLogger<ProjectService>.Instance, UnitTestFactory.CreateCompanyService(db));

        var extraSubmitterRole = db.CompanyRoles.Single(x => x.Name == RoleNames.Submitter);
        var extraSubmitter = UnitTestFactory.CreateMember("submitter-2", "submitter-user-2", scenario.Company.Id,
            extraSubmitterRole, "Extra", "Submitter");
        db.Users.Add(extraSubmitter.User);
        db.CompanyMembers.Add(extraSubmitter);
        await db.SaveChangesAsync();

        var result = await service.GetUnassignedMembersAsync(scenario.Project.Id, scenario.OtherDeveloper.Id);

        Assert.NotNull(result);
        Assert.True(result!.ContainsKey(RoleNames.Submitter));
        Assert.DoesNotContain(result.Values.SelectMany(x => x), member => member.Id == scenario.OtherDeveloper.Id);
        Assert.DoesNotContain(result.Values.SelectMany(x => x), member => member.Id == scenario.Developer.Id);
        Assert.DoesNotContain(result.Values.SelectMany(x => x), member => member.Id == scenario.Submitter.Id);
        Assert.Contains(result[RoleNames.Submitter], member => member.Id == extraSubmitter.Id);
    }

    [Fact]
    public async Task GetAssignedProjects_When_Role_Is_ProjectManager_Returns_Managed_Active_Projects_Only()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectService(db, NullLogger<ProjectService>.Instance, UnitTestFactory.CreateCompanyService(db));

        scenario.OtherProject.ProjectManagerId = scenario.ProjectManager.Id;
        scenario.OtherProject.IsArchived = true;
        await db.SaveChangesAsync();

        var projects = await service.GetAssignedProjects(scenario.ProjectManager.Id, RoleNames.ProjectManager,
            scenario.Company.Id);

        Assert.Single(projects);
        Assert.Equal(scenario.Project.Id, projects[0].Id);
    }

    [Fact]
    public async Task GetAssignedProjects_When_Role_Is_Admin_Returns_All_Active_Company_Projects()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectService(db, NullLogger<ProjectService>.Instance, UnitTestFactory.CreateCompanyService(db));

        scenario.OtherProject.IsArchived = false;
        await db.SaveChangesAsync();

        var projects = await service.GetAssignedProjects(scenario.Admin.Id, RoleNames.Admin, scenario.Company.Id);

        Assert.Equal(2, projects.Count);
        Assert.Contains(projects, project => project.Id == scenario.Project.Id);
        Assert.Contains(projects, project => project.Id == scenario.OtherProject.Id);
    }

    [Fact]
    public async Task GetAssignedProjects_When_Role_Is_Developer_Returns_Assigned_Active_Projects_Only()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var scenario = await UnitTestFactory.CreateAuthorizationScenarioAsync(db);
        var service = new ProjectService(db, NullLogger<ProjectService>.Instance, UnitTestFactory.CreateCompanyService(db));

        scenario.OtherProject.AssignedMembers.Add(scenario.Developer);
        scenario.Developer.AssignedProjects.Add(scenario.OtherProject);
        scenario.OtherProject.IsArchived = true;
        await db.SaveChangesAsync();

        var projects = await service.GetAssignedProjects(scenario.Developer.Id, RoleNames.Developer, scenario.Company.Id);

        Assert.Single(projects);
        Assert.Equal(scenario.Project.Id, projects[0].Id);
    }
}
