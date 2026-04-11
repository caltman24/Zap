namespace Zap.Tests.Features.Companies;

public sealed class GetCompanyInfoTests : IntegrationTestBase
{
    [Fact]
    public async Task Get_Company_Info_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CompanyTestData.CreateTestCompanyAsync(_db, userId, user);
        var client = _app.CreateClient(userId);

        var response = await client.GetFromJsonAsync<CompanyInfoDto>("/company/info");

        Assert.NotNull(response);
        Assert.Equal(company.Name, response.Name);
        Assert.Equal(company.Description, response.Description);
    }

    [Fact]
    public async Task Get_Company_Info_As_Developer_Returns_Forbidden()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        await CompanyTestData.CreateTestCompanyAsync(_db, userId, user, role: RoleNames.Developer);
        var client = _app.CreateClient(userId, RoleNames.Developer);

        var response = await client.GetAsync("/company/info");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_Company_Info_Unauthorized_Returns_401_Unauthorized()
    {
        var client = _app.CreateClient();
        var response = await client.GetAsync("/company/info");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_Company_Info_With_No_Relation_Returns_400_BadRequest()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);

        var client = _app.CreateClient(userId);
        var response = await client.GetAsync("/company/info");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}