namespace Zap.Tests.Features.Authentication;

public sealed class RegisterUserTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_User_Returns_Success()
    {
        var registerRequest = new RegisterRequest(
            "test2019@test.com",
            "@Password1",
            "Test",
            "User");

        var client = _app.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/register", registerRequest);

        Assert.True(response.IsSuccessStatusCode);

        var user = _db.Users.First(user => user.Email == registerRequest.Email);
        Assert.NotNull(user);
        Assert.Equal(registerRequest.Email, user.UserName);
        Assert.Equal(registerRequest.FirstName, user.FirstName);
        Assert.Equal(registerRequest.LastName, user.LastName);
    }

    [Fact]
    public async Task Register_User_Without_Name_Returns_400_BadRequest()
    {
        var registerRequest = new RegisterRequest(
            "test@test.com",
            "@pwd",
            null,
            null);

        var client = _app.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_Existing_User_Returns_400_BadRequest()
    {
        var registerRequest = new RegisterRequest(
            "test2020@test.com",
            "@Password1",
            "Test",
            "User");

        var client = _app.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync("/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    private sealed record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);
}