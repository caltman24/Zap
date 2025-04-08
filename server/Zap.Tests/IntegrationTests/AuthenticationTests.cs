namespace Zap.Tests.IntegrationTests;

public class AuthenticationTests
{
    private Task<(ZapApplication app, AppDbContext db, HttpClient client)> SetupTestEnvironment()
    {
        var app = new ZapApplication();
        var db = app.CreateAppDbContext();
        var client = app.CreateClient();
        
        return Task.FromResult((app, db, client));
    }

    private RegisterRequest CreateValidRegisterRequest(string? email = null)
    {
        return new RegisterRequest(
            Email: email ?? $"test{Guid.NewGuid()}@test.com",
            Password: "@Password1",
            FirstName: "Test",
            LastName: "User");
    }

    [Fact]
    public async Task Register_User_Returns_Success()
    {
        var (app, db, client) = await SetupTestEnvironment();
        var registerRequest = CreateValidRegisterRequest();

        var res = await client.PostAsJsonAsync("/auth/register", registerRequest);

        Assert.True(res.IsSuccessStatusCode);

        var user = db.Users.First(x => x.Email == registerRequest.Email);
        Assert.NotNull(user);

        Assert.Equal(registerRequest.Email, user.UserName);
        Assert.Equal(registerRequest.FirstName, user.FirstName);
        Assert.Equal(registerRequest.LastName, user.LastName);
        
        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task Register_User_Without_Name_Returns_400_BadRequest()
    {
        var (app, db, client) = await SetupTestEnvironment();
        
        var registerRequest = new RegisterRequest(
            Email: "test@test.com",
            Password: "@pwd", // Fails validation. Minimum 6
            FirstName: null, // Fails - Null
            LastName: null); // Fails - Null

        var res = await client.PostAsJsonAsync("/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        
        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task Register_Existing_User_Returns_400_BadRequest()
    {
        var (app, db, client) = await SetupTestEnvironment();
        var registerRequest = CreateValidRegisterRequest("test2020@test.com");

        var res = await client.PostAsJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var badRes = await client.PostAsJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.BadRequest, badRes.StatusCode);
        
        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    private async Task<(string userId, string email, string password)> CreateTestUser(ZapApplication app)
    {
        var userId = Guid.NewGuid().ToString();
        var email = userId + "@test.com";
        var password = "Password22!";
        
        await app.CreateUserAsync(userId, email, password);
        
        return (userId, email, password);
    }

    [Fact]
    public async Task SignIn_Email_Password_Returns_200_Success()
    {
        var (app, db, client) = await SetupTestEnvironment();
        var (_, email, password) = await CreateTestUser(app);

        var signInRequest = new SignInRequest(email, password);
        var res = await client.PostAsJsonAsync("/auth/signin", signInRequest);

        Assert.True(res.IsSuccessStatusCode);
        
        await app.DisposeAsync();
        await db.DisposeAsync();
    }

    [Fact]
    public async Task SignIn_Invalid_Email_Password_Returns_400_BadRequest()
    {
        var (app, db, client) = await SetupTestEnvironment();
        var (_, email, password) = await CreateTestUser(app);

        var signInRequest = new SignInRequest("Nottheemail@test.com", password);
        var res = await client.PostAsJsonAsync("/auth/signin", signInRequest);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        
        await app.DisposeAsync();
        await db.DisposeAsync();
    } 
    
    [Fact]
    public async Task Send_Unauthenticated_Request_Returns_401_Unauthorized()
    {
        var (app, db, client) = await SetupTestEnvironment();

        var res = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        
        await app.DisposeAsync();
        await db.DisposeAsync();
    }
}

internal record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);

internal record SignInRequest(string Email, string Password);
