namespace Zap.Tests.Features.Companies;

public sealed class RegisterCompanyTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_Company_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);

        var client = _app.CreateClient(userId);
        var response = await client.PostAsJsonAsync("/company/register",
            new RegisterCompanyRequest("Test Company", "Description"));

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Register_Company_With_Existing_Relation_Returns400_BadRequest()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);

        var client = _app.CreateClient(userId);
        var request = new RegisterCompanyRequest("Test Company", "Description");
        var response = await client.PostAsJsonAsync("/company/register", request);
        Assert.True(response.IsSuccessStatusCode);

        var duplicateResponse = await client.PostAsJsonAsync("/company/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    private sealed record RegisterCompanyRequest(string Name, string Description);
}