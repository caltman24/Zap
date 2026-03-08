using System.Net.Http.Headers;

namespace Zap.Tests.IntegrationTests;

public class DemoEnvironmentTests : IAsyncDisposable
{
    private readonly ZapApplication _app;
    private readonly AppDbContext _db;

    public DemoEnvironmentTests()
    {
        _app = new ZapApplication();
        _db = _app.CreateAppDbContext();
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
        await _db.DisposeAsync();
    }

    [Theory]
    [InlineData("admin", RoleNames.Admin)]
    [InlineData("projectManager", RoleNames.ProjectManager)]
    [InlineData("developer", RoleNames.Developer)]
    [InlineData("submitter", RoleNames.Submitter)]
    public async Task SignIn_Demo_User_Returns_Expected_Role(string demoRole, string expectedRole)
    {
        var client = _app.CreateClient();

        var signInResponse = await client.PostAsJsonAsync("/auth/signin-demo", new DemoSignInRequest(demoRole));
        Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);

        var tokenResponse = await signInResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokenResponse);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(tokenResponse!.TokenType, tokenResponse.AccessToken);

        var infoResponse = await client.GetFromJsonAsync<UserInfoResponse>("/user/info");

        Assert.NotNull(infoResponse);
        Assert.Equal(expectedRole, infoResponse!.Role);
        Assert.NotNull(infoResponse.CompanyId);
        Assert.Contains("demo-", infoResponse.Email);
    }

    [Fact]
    public async Task SignIn_Demo_User_With_Invalid_Role_Returns_400_BadRequest()
    {
        var client = _app.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/signin-demo", new DemoSignInRequest("owner"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

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
        await _app.DisposeAsync();
        await _db.DisposeAsync();

        var configuredApp = new ZapApplication(new Dictionary<string, string?>
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

        Assert.Equal(1, await db.Companies.CountAsync(c => c.IsDemo));
        Assert.Equal(4, await db.Users.CountAsync(u => u.IsDemo));
        Assert.True(await db.Tickets.AnyAsync(t => t.Project.Company.IsDemo));

        await configuredApp.DisposeAsync();
    }

    private sealed record DemoSignInRequest(string Role);

    private sealed record TokenResponse(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);

    private sealed record UserInfoResponse(string Email, string Role, string? CompanyId);
}
