using System.Net.Http.Headers;
using Amazon.S3;
using Amazon.S3.Model;
using dotenv.net.Utilities;
using Zap.Tests.Helpers;

namespace Zap.Tests.IntegrationTests;

public class UploadFileTests : IAsyncDisposable
{
    private readonly ZapApplication _app;
    private readonly AppDbContext _db;
    private readonly IAmazonS3 _s3Client;

    public UploadFileTests()
    {
        _app = new ZapApplication();
        _db = _app.CreateAppDbContext();
        _s3Client = _app.Services.GetRequiredService<IAmazonS3>();
    }


    [Fact]
    public async Task Update_Company_With_Image_Invalid_MIME_As_Admin_Returns_400_BadRequest()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        await CompaniesTests.CreateTestCompany(_db, userId, user, role: RoleNames.Admin);
        var client = _app.CreateClient(userId, role: RoleNames.Admin);

        //Image
        await using var imageStream = File.OpenRead("./test-image.jpg");

        // Create multipart form data content
        using var content = CreateUploadFormContent(new UploadFormRequest(
            "Name",
            "Description",
            false,
            imageStream,
            false)); // don't add file content type to fail mime type validation

        // Make sure content type is set correctly
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var res = await client.PutAsync("/company/info", content);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Update_Company_With_Image_Valid_As_Admin_Returns_Success()
    {
        var userId = Guid.NewGuid().ToString();
        await _app.CreateUserAsync(userId);
        var user = await _db.Users.FindAsync(userId);
        Assert.NotNull(user);

        await CompaniesTests.CreateTestCompany(_db, userId, user);
        var client = _app.CreateClient(userId, role: RoleNames.Admin);

        //Image
        await using var imageStream = File.OpenRead("./test-image.jpg");

        // Create multipart form data content
        using var content = CreateUploadFormContent(new UploadFormRequest(
            "Name",
            "Description",
            false,
            imageStream));

        // Make sure content type is set correctly
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        var res = await client.PutAsync("/company/info", content);

        Assert.True(res.IsSuccessStatusCode);

        await _s3Client.ClearTestBucketAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _s3Client.Dispose();
        await _app.DisposeAsync();
        await _db.DisposeAsync();
    }

    private static MultipartFormDataContent CreateUploadFormContent(UploadFormRequest request)
    {
        // Create multipart form data content
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.Name), "Name");
        content.Add(new StringContent(request.Description), "Description");
        content.Add(new StringContent(request.RemoveLogo.ToString()), "RemoveLogo");
        content.Add(new StringContent(request.WebsiteUrl), "WebsiteUrl");

        if (request.FileStream == null) return content;

        const int maxSizeKb = 50 * 1024; // 50kb
        if (request.FileStream.Length > maxSizeKb)
        {
            throw new ArgumentException($"The file size is too large. {maxSizeKb}kb Max");
        }

        //Image
        var streamContent = new StreamContent(request.FileStream);
        if (request.AddFileContentType) streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(streamContent, "file", Path.GetFileName(request.FileStream.Name));

        return content;
    }

    private record UploadFormRequest(
        string Name,
        string Description,
        bool RemoveLogo,
        FileStream? FileStream,
        bool AddFileContentType = true,
        string WebsiteUrl = "https://example.com");
}
