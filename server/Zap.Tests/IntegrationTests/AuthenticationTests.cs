namespace Zap.Tests.IntegrationTests;

public class AuthenticationTests : IAsyncDisposable
{
    private readonly ZapApplication _app;
    private readonly AppDbContext _db;

    public AuthenticationTests()
    {
        _app = new ZapApplication();
        _db = _app.CreateAppDbContext();
    }

    public async ValueTask DisposeAsync()
    {
        await _app.DisposeAsync();
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task Register_User_Returns_Success()
    {
        var registerRequest = new RegisterRequest(
            "test2019@test.com",
            "@Password1",
            "Test",
            "User");

        var client = _app.CreateClient();
        var res = await client.PostAsJsonAsync("/auth/register", registerRequest);
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
            "test@test.com",
            "@pwd", // Fails validation. Minimum 6
            null, // Fails - Null
            null); // Fails - Null

        var client = _app.CreateClient();
        var res = await client.PostAsJsonAsync("/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
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
        var res = await client.PostAsJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var badRes = await client.PostAsJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.BadRequest, badRes.StatusCode);
    }


    [Fact]
    public async Task SignIn_Email_Password_Returns_200_Success()
    {
        var userId = Guid.NewGuid().ToString();
        var email = userId + "@test.com";
        var password = "Password22!";
        await _app.CreateUserAsync(userId, email, password);

        var signInRequest = new SignInRequest(email, password);
        var client = _app.CreateClient();
        var res = await client.PostAsJsonAsync("/auth/signin", signInRequest);

        Assert.True(res.IsSuccessStatusCode);
    }

    [Fact]
    public async Task SignIn_Invalid_Email_Password_Returns_400_BadRequest()
    {
        var userId = Guid.NewGuid().ToString();
        var email = userId + "@test.com";
        var password = "Password22!";
        await _app.CreateUserAsync(userId, email, password);

        var signInRequest = new SignInRequest("Nottheemail@test.com", password);
        var client = _app.CreateClient();
        var res = await client.PostAsJsonAsync("/auth/signin", signInRequest);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Send_Unauthenticated_Request_Returns_401_Unauthorized()
    {
        var client = _app.CreateClient();
        var res = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}

internal record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);

internal record SignInRequest(string Email, string Password);