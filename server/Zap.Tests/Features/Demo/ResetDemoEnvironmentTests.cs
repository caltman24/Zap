namespace Zap.Tests.Features.Demo;

public sealed class ResetDemoEnvironmentTests : IntegrationTestBase
{
    [Fact]
    public async Task Reset_Demo_Environment_When_Disabled_Returns_403_Forbidden()
    {
        var client = _app.CreateClient();
        var response = await client.PostAsync("/demo/reset", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Reset_Demo_Environment_With_Valid_Config_Recreates_Demo_Data()
    {
        await using var configuredApp = new ZapApplication(new Dictionary<string, string?>
        {
            ["Demo:EnableReset"] = bool.TrueString,
            ["Demo:ResetKey"] = "demo-reset-secret"
        });
        await using var db = configuredApp.CreateAppDbContext();
        var client = configuredApp.CreateClient();

        var signInResponse = await client.PostAsJsonAsync("/auth/signin-demo", new DemoSignInRequest("admin"));
        Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);

        db.Tickets.RemoveRange(db.Tickets);
        await db.SaveChangesAsync();

        var resetRequest = new HttpRequestMessage(HttpMethod.Post, "/demo/reset");
        resetRequest.Headers.Add("X-Demo-Reset-Key", "demo-reset-secret");

        var resetResponse = await client.SendAsync(resetRequest);

        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);
        Assert.Equal(1, await db.Companies.CountAsync(company => company.IsDemo));
        Assert.Equal(4, await db.Users.CountAsync(user => user.IsDemo));
        Assert.True(await db.Tickets.AnyAsync(ticket => ticket.Project.Company.IsDemo));
    }

    private sealed record DemoSignInRequest(string Role);
}