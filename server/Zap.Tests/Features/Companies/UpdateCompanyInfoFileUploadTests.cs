using System.Net.Http.Headers;
using Amazon.S3;
using Zap.Tests.Helpers;

namespace Zap.Tests.Features.Companies;

public sealed class UpdateCompanyInfoFileUploadTests : IntegrationTestBase
{
    private readonly string _bucketName;
    private readonly IAmazonS3 _s3Client;

    public UpdateCompanyInfoFileUploadTests()
    {
        _s3Client = _app.Services.GetRequiredService<IAmazonS3>();
        _bucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET")
                      ?? throw new InvalidOperationException(
                          "S3 integration tests require AWS_S3_BUCKET. Configure server/Zap.Tests/.env.test.");

        S3BucketTestHelper.EnsureSafeTestBucketName(_bucketName);
    }

    public override async ValueTask DisposeAsync()
    {
        _s3Client.Dispose();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Update_Company_With_Image_Invalid_MIME_As_Admin_Returns_400_BadRequest()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        await CompanyTestData.CreateTestCompanyAsync(_db, userId, user, role: RoleNames.Admin);
        var client = _app.CreateClient(userId);

        await using var imageStream = File.OpenRead("./test-image.jpg");
        using var content =
            CreateUploadFormContent(new UploadFormRequest("Name", "Description", false, imageStream, false));

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var response = await client.PutAsync("/company/info", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_Company_With_Image_Valid_As_Admin_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CompanyTestData.CreateTestCompanyAsync(_db, userId, user);
        var client = _app.CreateClient(userId);

        await using var imageStream = File.OpenRead("./test-image.jpg");
        using var content = CreateUploadFormContent(new UploadFormRequest("Name", "Description", false, imageStream));

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var response = await client.PutAsync("/company/info", content);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedCompany = await _db.Companies.SingleAsync(x => x.Id == company.Id);
        Assert.NotNull(updatedCompany.LogoUrl);
        Assert.NotNull(updatedCompany.LogoKey);

        await S3BucketTestHelper.ClearTestBucketAsync(_s3Client, _bucketName);
    }

    [Fact]
    public async Task Update_Company_With_RemoveLogo_True_As_Admin_Clears_Existing_Logo_Metadata()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        var company = await CompanyTestData.CreateTestCompanyAsync(_db, userId, user);
        var client = _app.CreateClient(userId);

        await using (var imageStream = File.OpenRead("./test-image.jpg"))
        {
            using var uploadContent =
                CreateUploadFormContent(new UploadFormRequest("Name", "Description", false, imageStream));

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
            var uploadResponse = await client.PutAsync("/company/info", uploadContent);
            Assert.Equal(HttpStatusCode.NoContent, uploadResponse.StatusCode);
        }

        using var removeContent = CreateUploadFormContent(new UploadFormRequest(
            "Renamed Company",
            "Updated Description",
            true,
            null));

        var removeResponse = await client.PutAsync("/company/info", removeContent);

        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        _db.ChangeTracker.Clear();
        var updatedCompany = await _db.Companies.SingleAsync(x => x.Id == company.Id);
        Assert.Null(updatedCompany.LogoUrl);
        Assert.Null(updatedCompany.LogoKey);
        Assert.Equal("Renamed Company", updatedCompany.Name);
        Assert.Equal("Updated Description", updatedCompany.Description);

        await S3BucketTestHelper.ClearTestBucketAsync(_s3Client, _bucketName);
    }

    private static MultipartFormDataContent CreateUploadFormContent(UploadFormRequest request)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.Name), "Name");
        content.Add(new StringContent(request.Description), "Description");
        content.Add(new StringContent(request.RemoveLogo.ToString()), "RemoveLogo");
        content.Add(new StringContent(request.WebsiteUrl), "WebsiteUrl");

        if (request.FileStream == null) return content;

        const int maxSizeKb = 50 * 1024;
        if (request.FileStream.Length > maxSizeKb)
            throw new ArgumentException($"The file size is too large. {maxSizeKb}kb Max");

        var streamContent = new StreamContent(request.FileStream);
        if (request.AddFileContentType) streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

        content.Add(streamContent, "file", Path.GetFileName(request.FileStream.Name));
        return content;
    }

    private sealed record UploadFormRequest(
        string Name,
        string Description,
        bool RemoveLogo,
        FileStream? FileStream,
        bool AddFileContentType = true,
        string WebsiteUrl = "https://example.com");
}
