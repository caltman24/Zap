namespace Zap.Tests.Features.Authentication;

public sealed class SignInUserTests : IntegrationTestBase
{
    [Fact]
    public async Task SignIn_Email_Password_Returns_200_Success()
    {
        var userId = Guid.NewGuid().ToString();
        var email = userId + "@test.com";
        var password = "Password22!";
        await _app.CreateUserAsync(userId, email, password);

        var client = _app.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/signin", new SignInRequest(email, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SignIn_Invalid_Credentials_Returns_400_BadRequest()
    {
        var userId = Guid.NewGuid().ToString();
        var email = userId + "@test.com";
        var password = "Password22!";
        await _app.CreateUserAsync(userId, email, password);

        var client = _app.CreateClient();
        var response =
            await client.PostAsJsonAsync("/auth/signin", new SignInRequest("Nottheemail@test.com", password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SignIn_Invalid_Request_Returns_400_BadRequest()
    {
        var client = _app.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/signin", new SignInRequest("not-an-email", ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record SignInRequest(string Email, string Password);
}
