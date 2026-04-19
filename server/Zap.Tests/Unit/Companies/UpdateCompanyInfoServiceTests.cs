using Microsoft.Extensions.Logging.Abstractions;
using Zap.Api.Features.FileUpload.Services;
using Zap.Tests.Unit.TestHelpers;

namespace Zap.Tests.Unit.Companies;

public sealed class UpdateCompanyInfoServiceTests
{
    [Fact]
    public async Task UpdateCompanyInfoAsync_When_RemoveLogo_Is_True_Clears_Metadata_And_Deletes_File()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var fileUpload = new RecordingFileUploadService();
        var service = new CompanyService(db, fileUpload, NullLogger<CompanyService>.Instance);
        var company = new Company
        {
            Id = "company-1",
            Name = "Company",
            Description = "Description",
            LogoKey = "logo-key",
            LogoUrl = "https://example.com/logo.png"
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var updated = await service.UpdateCompanyInfoAsync(new UpdateCompanyInfoDto(company.Id, "Renamed", "Updated",
            "https://example.com", null, true));

        Assert.True(updated);
        Assert.Equal(["logo-key"], fileUpload.DeletedKeys);
        var persisted = await db.Companies.SingleAsync(x => x.Id == company.Id);
        Assert.Null(persisted.LogoKey);
        Assert.Null(persisted.LogoUrl);
        Assert.Equal("Renamed", persisted.Name);
        Assert.Equal("Updated", persisted.Description);
        Assert.Equal("https://example.com", persisted.WebsiteUrl);
    }

    [Fact]
    public async Task UpdateCompanyInfoAsync_When_Logo_Upload_Succeeds_Stores_New_Metadata()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var fileUpload = new RecordingFileUploadService
        {
            UploadResult = ("https://example.com/new-logo.png", "new-key")
        };
        var service = new CompanyService(db, fileUpload, NullLogger<CompanyService>.Instance);
        var company = new Company
        {
            Id = "company-1",
            Name = "Company",
            Description = "Description"
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var logo = new FormFile(Stream.Null, 0, 0, "logo", "logo.png");
        var updated = await service.UpdateCompanyInfoAsync(new UpdateCompanyInfoDto(company.Id, "Renamed", "Updated",
            null, logo, false));

        Assert.True(updated);
        var persisted = await db.Companies.SingleAsync(x => x.Id == company.Id);
        Assert.Equal("https://example.com/new-logo.png", persisted.LogoUrl);
        Assert.Equal("new-key", persisted.LogoKey);
    }

    [Fact]
    public async Task UpdateCompanyInfoAsync_When_Delete_Fails_Returns_False_And_Leaves_Logo_Intact()
    {
        await using var db = UnitTestFactory.CreateDbContext();
        var fileUpload = new RecordingFileUploadService { ThrowOnDelete = true };
        var service = new CompanyService(db, fileUpload, NullLogger<CompanyService>.Instance);
        var company = new Company
        {
            Id = "company-1",
            Name = "Company",
            Description = "Description",
            LogoKey = "logo-key",
            LogoUrl = "https://example.com/logo.png"
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var updated = await service.UpdateCompanyInfoAsync(new UpdateCompanyInfoDto(company.Id, "Renamed", "Updated",
            null, null, true));

        Assert.False(updated);
        var persisted = await db.Companies.SingleAsync(x => x.Id == company.Id);
        Assert.Equal("logo-key", persisted.LogoKey);
        Assert.Equal("https://example.com/logo.png", persisted.LogoUrl);
    }

    private sealed class RecordingFileUploadService : IFileUploadService
    {
        public List<string> DeletedKeys { get; } = [];
        public (string url, string key) UploadResult { get; set; } = ("https://example.com/logo.png", "logo-key");
        public bool ThrowOnDelete { get; set; }

        public Task<(string url, string key)> UploadAvatarAsync(IFormFile file, string? oldKey = null)
        {
            throw new NotSupportedException();
        }

        public Task<(string url, string key)> UploadCompanyLogoAsync(IFormFile file, string? oldKey = null)
        {
            return Task.FromResult(UploadResult);
        }

        public Task<(string url, string key)> UploadAttachmentAsync(IFormFile file)
        {
            throw new NotSupportedException();
        }

        public Task DeleteFileAsync(string key)
        {
            if (ThrowOnDelete) throw new InvalidOperationException("delete failed");
            DeletedKeys.Add(key);
            return Task.CompletedTask;
        }
    }
}
