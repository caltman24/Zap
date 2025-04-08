using Xunit.Abstractions;

namespace Zap.Tests.IntegrationTests;

public class RegisterTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RegisterTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Register_User_Returns_Success()
    {
        var registerRequest = new RegisterRequest(
            Email: "test2019@test.com",
            Password: "@Password1",
            FirstName: "Test",
            LastName: "User");


        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        var client = app.CreateClient();

        var res = await client.PostAsJsonAsync("/auth/register", registerRequest);

        Assert.True(res.IsSuccessStatusCode);

        var user = db.Users.First(x => x.Email == registerRequest.Email);
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


        await using var app = new ZapApplication();
        var client = app.CreateClient();

        var res = await client.PostAsJsonAsync("/auth/register", registerRequest);

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

        await using var app = new ZapApplication();
        var client = app.CreateClient();

        var res = await client.PostAsJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var badRes = await client.PostAsJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.BadRequest, badRes.StatusCode);
    }

    [Fact]
    public async Task Register_Company_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId);
        
        var client = app.CreateClient(userId);

        var registerRequest = new RegisterCompanyRequest("Test Company", "Description");
        var res = await client.PostAsJsonAsync("/company/register", registerRequest);

        Assert.True(res.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Register_Company_With_Existing_Relation_Returns400_BadRequest()
    {
        var userId = Guid.NewGuid().ToString();

        await using var app = new ZapApplication();
        await using var db = app.CreateAppDbContext();
        await app.CreateUserAsync(userId);

        var client = app.CreateClient(userId);

        var registerRequest = new RegisterCompanyRequest("Test Company", "Description");
        var res = await client.PostAsJsonAsync("/company/register", registerRequest);
        Assert.True(res.IsSuccessStatusCode);

        var badRes = await client.PostAsJsonAsync("/company/register", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, badRes.StatusCode);
    }
}

internal record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);

internal record RegisterCompanyRequest(string Name, string Description);