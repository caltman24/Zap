using Zap.Api.Features.Users.Endpoints;

namespace Zap.Tests.Features.Users;

public sealed class GetUserInfoTests : IntegrationTestBase
{
    [Fact]
    public async Task Get_User_Info_As_Project_Manager_Returns_Profile_And_Permissions()
    {
        var userId = Guid.NewGuid().ToString();
        var email = $"{userId}@test.com";
        await _app.CreateUserAsync(userId, email);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CompanyTestData.CreateTestCompanyAsync(_db, userId, user, role: RoleNames.ProjectManager);
        var memberId = await _db.CompanyMembers
            .Where(member => member.UserId == userId && member.CompanyId == company.Id)
            .Select(member => member.Id)
            .SingleAsync();

        var client = _app.CreateClient(userId, RoleNames.ProjectManager);
        var response = await client.GetFromJsonAsync<GetUserInfo.Response>("/user/info");

        Assert.NotNull(response);
        Assert.Equal(userId, response!.Id);
        Assert.Equal(email, response.Email);
        Assert.Equal(RoleNames.ProjectManager, response.Role);
        Assert.Equal(company.Id, response.CompanyId);
        Assert.Equal(memberId, response.MemberId);
        Assert.Contains("project.create", response.Permissions);
        Assert.Contains("project.viewAssigned", response.Permissions);
        Assert.DoesNotContain("company.edit", response.Permissions);
    }

    [Fact]
    public async Task Get_User_Info_Without_Company_Relation_Returns_Profile_With_Empty_Role_And_Permissions()
    {
        var userId = Guid.NewGuid().ToString();
        var email = $"{userId}@test.com";
        await _app.CreateUserAsync(userId, email);

        var client = _app.CreateClient(userId);
        var response = await client.GetFromJsonAsync<GetUserInfo.Response>("/user/info");

        Assert.NotNull(response);
        Assert.Equal(userId, response!.Id);
        Assert.Equal(email, response.Email);
        Assert.Equal(string.Empty, response.Role);
        Assert.Null(response.CompanyId);
        Assert.Null(response.MemberId);
        Assert.Empty(response.Permissions);
    }

    [Fact]
    public async Task Get_User_Info_Unauthenticated_Returns_404_NotFound()
    {
        var client = _app.CreateClient();

        var response = await client.GetAsync("/user/info");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
