using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Users;

public sealed class UserPermissionServiceTests
{
    private readonly UserPermissionService _service = new();

    [Theory]
    [InlineData(RoleNames.Admin, new[] { "company.edit", "project.create", "project.viewAll", "project.viewArchived", "project.assignPm", "ticket.create" })]
    [InlineData(RoleNames.ProjectManager, new[] { "project.create", "project.viewAll", "project.viewAssigned", "project.viewArchived", "ticket.create" })]
    [InlineData(RoleNames.Developer, new[] { "project.viewAssigned" })]
    [InlineData(RoleNames.Submitter, new[] { "project.viewAssigned", "ticket.create" })]
    public void GetPermissions_When_Role_Is_Known_Returns_Exact_Set(string roleName, string[] expectedPermissions)
    {
        var currentUser = UnitTestFactory.CreateCurrentUser(roleName);

        var permissions = _service.GetPermissions(currentUser);

        Assert.Equal(expectedPermissions, permissions);
    }

    [Fact]
    public void GetPermissions_When_Member_Is_Missing_Returns_Empty()
    {
        var currentUser = UnitTestFactory.CreateCurrentUser(null);

        var permissions = _service.GetPermissions(currentUser);

        Assert.Empty(permissions);
    }

    [Fact]
    public void GetPermissions_When_Role_Is_Unknown_Returns_Empty()
    {
        var currentUser = UnitTestFactory.CreateCurrentUser("Auditor");

        var permissions = _service.GetPermissions(currentUser);

        Assert.Empty(permissions);
    }
}
