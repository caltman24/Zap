using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Companies;

public sealed class CompanyServiceTests
{
    [Fact]
    public void GetMembersPerRole_Groups_Members_By_Role()
    {
        using var db = UnitTestFactory.CreateDbContext();
        var service = UnitTestFactory.CreateCompanyService(db);
        var adminRole = UnitTestFactory.CreateRole(RoleNames.Admin);
        var developerRole = UnitTestFactory.CreateRole(RoleNames.Developer);

        var members = new List<CompanyMember>
        {
            UnitTestFactory.CreateMember("admin-1", "admin-user-1", "company-1", adminRole, "Admin", "One"),
            UnitTestFactory.CreateMember("dev-1", "dev-user-1", "company-1", developerRole, "Dev", "One"),
            UnitTestFactory.CreateMember("dev-2", "dev-user-2", "company-1", developerRole, "Dev", "Two")
        };

        var grouped = service.GetMembersPerRole(members);

        Assert.Equal(2, grouped.Count);
        Assert.True(grouped.ContainsKey(RoleNames.Admin));
        Assert.True(grouped.ContainsKey(RoleNames.Developer));
        Assert.Single(grouped[RoleNames.Admin]);
        Assert.Equal(2, grouped[RoleNames.Developer].Count);
        Assert.Contains(grouped[RoleNames.Developer], x => x.Name == "Dev One");
        Assert.Contains(grouped[RoleNames.Developer], x => x.Name == "Dev Two");
    }
}
