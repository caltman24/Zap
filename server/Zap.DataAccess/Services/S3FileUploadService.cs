using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Zap.DataAccess.Configuration;

namespace Zap.DataAccess.Services;

public class S3FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;

    public S3FileUploadService(IAmazonS3 s3Client, IOptions<S3Options> s3Options)
    {
        _s3Client = s3Client;
        _bucketName = s3Options.Value.BucketName;
        _region = s3Options.Value.Region;
    }


    public Task<(string url, string key)> UploadAvatarAsync(IFormFile file)
    {
        return UploadFileAsync(file, "users/avatars", 2);
    }

    public Task<(string url, string key)> UploadCompanyLogoAsync(IFormFile file)
    {
        return UploadFileAsync(file, "companies/logos", 2);
    }

    public Task<(string url, string key)> UploadAttachmentAsync(IFormFile file)
    {
        return UploadFileAsync(file, "attachments");
    }

    public async Task DeleteFileAsync(string key)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(request);
    }

    private async Task<(string url, string key)> UploadFileAsync(IFormFile file, string prefix, int maxSizeMb = 10)
    {
        var fileSizeBytes = file.Length;

        if (fileSizeBytes > maxSizeMb * 1024 * 1024) // To bytes
        {
            throw new Exception($"File size exceeds {maxSizeMb}MB");
        }

        var key = $"{prefix}/{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = file.OpenReadStream(),
            ContentType = file.ContentType
        };

        await _s3Client.PutObjectAsync(request);

        return ($"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}", key);
    }
}