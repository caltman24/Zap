using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Zap.DataAccess.Models;

namespace Zap.Tests;

public class IntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private const string TestEmail = "test@example.com";

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // The database will be dropped by the TestWebApplicationFactory
    }

    [Fact]
    public async Task Register_User_Returns_Success()
    {
        await using var application = new TestWebApplicationFactory();
        await using var db = application.CreateAppDbContext();

        var registerRequest = new RegisterRequest(
            Email: TestEmail,
            Password: "@pwd",
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
            Password: "@pwd",
            FirstName: null,
            LastName: null);
        
        var client = application.CreateDefaultClient();
        var res = await client.PostAsJsonAsync("/register/user", registerRequest);
        
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}

record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);