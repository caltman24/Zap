using System.Net.Http.Headers;

namespace Zap.Tests.Features.Authentication;

public sealed class RefreshTokensTests : IntegrationTestBase
{
    [Fact]
    public async Task Refresh_Tokens_With_Valid_Token_Returns_New_Tokens_And_Allows_Authorized_Request()
    {
        var client = _app.CreateClient();

        var signInResponse = await client.PostAsJsonAsync("/auth/signin-demo", new DemoSignInRequest("admin"));
        Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);

        var signInToken = await signInResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(signInToken);

        var refreshResponse = await client.PostAsJsonAsync("/auth/refresh", new RefreshRequest(signInToken!.RefreshToken));

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshedToken = await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(refreshedToken);
        Assert.False(string.IsNullOrWhiteSpace(refreshedToken!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshedToken.RefreshToken));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(refreshedToken.TokenType, refreshedToken.AccessToken);

        var userInfoResponse = await client.GetAsync("/user/info");
        Assert.Equal(HttpStatusCode.OK, userInfoResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_Tokens_With_Invalid_Token_Returns_401_Unauthorized()
    {
        var client = _app.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/refresh", new RefreshRequest("invalid-refresh-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed record DemoSignInRequest(string Role);

    private sealed record RefreshRequest(string RefreshToken);

    private sealed record TokenResponse(string TokenType, string AccessToken, int ExpiresIn, string RefreshToken);
}
