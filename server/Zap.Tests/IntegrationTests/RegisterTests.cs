using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Zap.Tests.IntegrationTests;

public class RegisterTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly ITestOutputHelper _testOutputHelper;
    private AppDbContext _db = null!;
    private HttpClient _client = null!;

    public RegisterTests(TestWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
    }

    public Task InitializeAsync()
    {
        _db = _factory.CreateAppDbContext();
        _client = _factory.CreateDefaultClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task Register_User_Returns_Success()
    {
        var registerRequest = new RegisterRequest(
            Email: "test2019@test.com",
            Password: "@Password1",
            FirstName: "Test",
            LastName: "User");

        var res = await _client.PostAsJsonAsync("/auth/register", registerRequest);

        Assert.True(res.IsSuccessStatusCode);

        var user = _db.Users.First(x => x.Email == registerRequest.Email);
        Assert.NotNull(user);

        Assert.Equal(registerRequest.Email, user.UserName);
        Assert.Equal(registerRequest.FirstName, user.FirstName);
        Assert.Equal(registerRequest.LastName, user.LastName);
    }

    [Fact]
    public async Task Register_User_Without_Name_Returns_400_BadRequest()
    {
        var registerRequest = new RegisterRequest(
            Email: "test@test.com",
            Password: "@pwd", // Fails validation. Minimum 6
            FirstName: null, // Fails - Null
            LastName: null); // Fails - Null

        var res = await _client.PostAsJsonAsync("/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Register_Existing_User_Returns_400_BadRequest()
    {
        var registerRequest = new RegisterRequest(
            Email: "test2020@test.com",
            Password: "@Password1",
            FirstName: "Test",
            LastName: "User");

        var res = await _client.PostAsJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var badRes = await _client.PostAsJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.BadRequest, badRes.StatusCode);
    }

    [Fact]
    public async Task Register_Company_Returns_Success()
    {
        var userId = "toast123";
        
        var registerRequest = new RegisterCompanyRequest("Test Company", "Description");
        await _factory.CreateUserAsync(userId);
        var client = _factory.CreateClient(userId);
        
        var res = await client.PostAsJsonAsync("/company/register", registerRequest);
        _testOutputHelper.WriteLine($"Response: {res.StatusCode}");
        
        Assert.True(res.IsSuccessStatusCode);
    }
}

record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);
record RegisterCompanyRequest(string Name, string Description);