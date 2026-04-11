namespace Zap.Tests.Features.Companies;

public sealed class RegisterCompanyTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_Company_Returns_204_NoContent_And_Persists_Company()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);

        var client = _app.CreateClient(userId);
        var response = await client.PostAsJsonAsync("/company/register",
            new RegisterCompanyRequest("Test Company", "Description"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var company = await _db.Companies
            .Include(x => x.Members)
            .ThenInclude(x => x.Role)
            .SingleAsync(x => x.OwnerId == userId);

        Assert.Equal("Test Company", company.Name);
        Assert.Equal("Description", company.Description);
        Assert.Contains(company.Members, member => member.UserId == userId && member.Role.Name == RoleNames.Admin);
    }

    [Fact]
    public async Task Register_Company_With_Existing_Relation_Returns400_BadRequest()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);

        var client = _app.CreateClient(userId);
        var request = new RegisterCompanyRequest("Test Company", "Description");
        var response = await client.PostAsJsonAsync("/company/register", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync("/company/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    private sealed record RegisterCompanyRequest(string Name, string Description);
}
