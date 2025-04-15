using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Zap.Api.Features.FileUpload.Configuration;

namespace Zap.Api.Features.FileUpload.Services;

public sealed class S3FileUploadService : IFileUploadService
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


    /// <summary>
    /// Uploads a user avatar to S3
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="oldKey">Old object key to remove</param>
    /// <returns>URL and key of the uploaded file</returns>
    public async Task<(string url, string key)> UploadAvatarAsync(IFormFile file, string? oldKey = null)
    {
        if (oldKey != null)
        {
            _logger.LogDebug("Deleting old user avatar: {Key}", oldKey);
            await DeleteFileAsync(oldKey);
        }

        return await UploadFileAsync(file, "users/avatars", 2);
    }

    /// <summary>
    /// Uploads a company logo to S3
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="oldKey">Old object key to remove</param>
    /// <returns>URL and key of the uploaded file</returns>
    public async Task<(string url, string key)> UploadCompanyLogoAsync(IFormFile file, string? oldKey = null)
    {
        if (oldKey != null)
        {
            _logger.LogDebug("Deleting old company logo: {Key}", oldKey);
            await DeleteFileAsync(oldKey);
        }

        return await UploadFileAsync(file, "companies/logos", 2);
    }

    public async Task<(string url, string key)> UploadAttachmentAsync(IFormFile file)
    {
        return await UploadFileAsync(file, "attachments");
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
    }

    private async Task<(string url, string key)> UploadFileAsync(IFormFile file, string prefix, int maxSizeMb = 10)
    {
        var fileSizeBytes = file.Length;

        if (fileSizeBytes > maxSizeMb * 1024 * 1024) // To bytes
        {
            throw new Exception($"File size exceeds {maxSizeMb}MB");
        }

        // Validate file type
        ValidateFileType(file);

        var key = $"{prefix}/{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";
        _logger.LogDebug("Generated S3 key: {Key}", key);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = file.OpenReadStream(),
            ContentType = file.ContentType
        };

        // Add checksum for integrity validation
        await using (var stream = file.OpenReadStream())
        {
            var checksum = CalculateChecksum(stream);
            request.ChecksumAlgorithm = ChecksumAlgorithm.SHA256;
            stream.Position = 0;
        }

        try
        {
            var res = await _s3Client.PutObjectAsync(request);
            if (res is not { HttpStatusCode: System.Net.HttpStatusCode.OK })
            {
                _logger.LogError("Failed to upload file: {Key}", key);
                throw new Exception("Failed to upload file");
            }

            _logger.LogInformation("Successfully uploaded file: {Key}, size: {SizeKB}KB", key, fileSizeBytes / 1024);
            return ($"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}", key);
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            _logger.LogError(amazonS3Exception, "Failed to upload file: {Key}", key);
            throw new Exception("Failed to upload file");
        }
    }

    private void ValidateFileType(IFormFile file)
    {
        // List of allowed MIME types
        var allowedTypes = new[]
        {
            "image/jpeg", "image/png", "image/gif", "image/webp",
            "application/pdf", "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        if (!allowedTypes.Contains(file.ContentType))
        {
            _logger.LogWarning("Invalid file type attempted: {ContentType}", file.ContentType);
            throw new Exception("Invalid file type. Only images, PDFs, and Office documents are allowed.");
        }

        // Additional validation for file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

        if (allowedExtensions.Contains(extension)) return;
        _logger.LogWarning("Invalid file extension attempted: {Extension}", extension);
        throw new Exception("Invalid file extension. Only images, PDFs, and Office documents are allowed.");
    }

    private string CalculateChecksum(Stream stream)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hash = sha256.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }
    }
}
