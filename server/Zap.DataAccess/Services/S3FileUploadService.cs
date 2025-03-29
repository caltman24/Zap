using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zap.DataAccess.Configuration;

namespace Zap.DataAccess.Services;

public class S3FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3FileUploadService> _logger;
    private readonly string _bucketName;
    private readonly string _region;

    public S3FileUploadService(IAmazonS3 s3Client, IOptions<S3Options> s3Options, ILogger<S3FileUploadService> logger)
    {
        _s3Client = s3Client;
        _bucketName = s3Options.Value.BucketName;
        _region = s3Options.Value.Region;
        _logger = logger;
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
        _logger.LogDebug("Deleting file: {Key}", key);

        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        try
        {
            await _s3Client.DeleteObjectAsync(request);
            _logger.LogDebug("Successfully deleted file: {Key}", key);
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError(amazonS3Exception, "Failed to delete file: {Key}", key);
            throw new Exception("Failed to delete file");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete file: {Key}", key);
            throw new Exception("Failed to delete file");
        }
    }

    private async Task<(string url, string key)> UploadFileAsync(IFormFile file, string prefix, int maxSizeMb = 10)
    {
        var fileSizeBytes = file.Length;

        if (fileSizeBytes > maxSizeMb * 1024 * 1024) // To bytes
        {
            throw new Exception($"File size exceeds {maxSizeMb}MB");
        }

        var key = $"{prefix}/{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";
        _logger.LogDebug("Generated S3 key: {Key}", key);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = file.OpenReadStream(),
            ContentType = file.ContentType
        };

        try
        {
            var res = await _s3Client.PutObjectAsync(request);
            if (res is not { HttpStatusCode: System.Net.HttpStatusCode.OK })
            {
                _logger.LogError("Failed to upload file: {Key}", key);
                throw new Exception("Failed to upload file");
            }

            _logger.LogDebug("Successfully uploaded file: {Key}, size: {SizeKB}KB", key, fileSizeBytes / 1024);
            return ($"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}", key);
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError(amazonS3Exception, "Failed to upload file: {Key}", key);
            throw new Exception("Failed to upload file");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to upload file: {Key}", key);
            throw new Exception("Failed to upload file");
        }
    }
}