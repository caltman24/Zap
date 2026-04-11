namespace Zap.Tests.Features.Authentication;

public sealed class AuthorizationSmokeTests : IntegrationTestBase
{
    [Fact]
    public async Task Send_Unauthenticated_Request_Returns_401_Unauthorized()
    {
        var client = _app.CreateClient();
        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}