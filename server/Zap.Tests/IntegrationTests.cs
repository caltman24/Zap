using System.Net;
using System.Net.Http.Json;

namespace Zap.Tests;

public class IntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private const string TestEmail = "test@example.com";

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;


    [Fact]
    public async Task Register_User_Returns_Success()
    {
        await using var application = new TestWebApplicationFactory();
        await using var db = application.CreateAppDbContext();

        var registerRequest = new RegisterRequest(
            Email: TestEmail,
            Password: "@Password1",
            FirstName: "Test",
            LastName: "User");

        var client = application.CreateDefaultClient();
        var res = await client.PostAsJsonAsync("/register/user", registerRequest);

        Assert.True(res.IsSuccessStatusCode);

        var user = db.Users.Single();
        Assert.NotNull(user);

        Assert.Equal(registerRequest.Email, user.UserName);
        Assert.Equal(registerRequest.Email, user.Email);
        Assert.Equal(registerRequest.FirstName, user.FirstName);
        Assert.Equal(registerRequest.LastName, user.LastName);
    }

    [Fact]
    public async Task Register_User_Without_Name_Returns_400_BadRequest()
    {
        await using var application = new TestWebApplicationFactory();
        await using var db = application.CreateAppDbContext();

        var registerRequest = new RegisterRequest(
            Email: TestEmail,
            Password: "@pwd", // Fails validation. Minimum 6
            FirstName: null, // Fails - Null
            LastName: null); // Fails - Null

        var client = application.CreateDefaultClient();
        var res = await client.PostAsJsonAsync("/register/user", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}

record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);