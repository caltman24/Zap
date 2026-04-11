using System.Net.Http.Headers;

namespace Zap.Tests.Features.Authentication;

public sealed class SignInDemoUserTests : IntegrationTestBase
{
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

    private sealed record DemoSignInRequest(string Role);

    private sealed record TokenResponse(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);

    private sealed record UserInfoResponse(string Email, string Role, string? CompanyId);
}