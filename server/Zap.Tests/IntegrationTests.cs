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
        using var scope = application.Services.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client = application.CreateDefaultClient();
        var res = await client.PostAsJsonAsync("/register/user",
            new { Email = "test@example.com", Password = "@pwd" });
        
        Assert.True(res.IsSuccessStatusCode);

        var user = db.Users.Single();
        Assert.NotNull(user);

        Assert.Equal("test@example.com", user.UserName);
    }
}