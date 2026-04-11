using System.Net.Http.Headers;

namespace Zap.Tests.Features.Companies;

public sealed class UpdateCompanyInfoTests : IntegrationTestBase
{
    [Fact]
    public async Task Update_Company_Without_Image_As_Admin_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        await CompanyTestData.CreateTestCompanyAsync(_db, userId, user, role: RoleNames.Admin);
        var client = _app.CreateClient(userId);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("New Company Name"), "Name");
        content.Add(new StringContent("New Description"), "Description");
        content.Add(new StringContent("false"), "RemoveLogo");
        content.Add(new StringContent("https://example.com"), "WebsiteUrl");

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var response = await client.PutAsync("/company/info", content);

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Update_Company_Without_Image_As_ProjectManager_Returns_Forbidden()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        await CompanyTestData.CreateTestCompanyAsync(_db, userId, user, role: RoleNames.ProjectManager);
        var client = _app.CreateClient(userId, RoleNames.ProjectManager);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("New Company Name"), "Name");
        content.Add(new StringContent("New Description"), "Description");
        content.Add(new StringContent("false"), "RemoveLogo");
        content.Add(new StringContent("https://example.com"), "WebsiteUrl");

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var response = await client.PutAsync("/company/info", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}