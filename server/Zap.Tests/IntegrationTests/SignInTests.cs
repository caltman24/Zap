using System.Net.Http.Json;
using Xunit.Abstractions;
using Zap.Api.Data;

namespace Zap.Tests.IntegrationTests;

public class SignInTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SignInTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task SignIn_Email_Password_Returns_200_Success()
    {
        var userId = Guid.NewGuid().ToString();
        var email = userId + "@test.com";
        var password = "Password22!";

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId, email, password);
        
        var client = app.CreateClient();

        var signInRequest = new SignInRequest(email, password);
        var res = await client.PostAsJsonAsync("/auth/signin", signInRequest);
        var r = await res.Content.ReadAsStringAsync();

        Assert.True(res.IsSuccessStatusCode);
    }
}

internal record SignInRequest(string Email, string Password);