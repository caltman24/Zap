using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Zap.DataAccess.Services;

public class S3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:BucketName"]!;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string prefix)
    {
        var fileName = $"{prefix}/{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = memoryStream,
            ContentType = file.ContentType
        };

        await _s3Client.PutObjectAsync(request);

        return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
    }
}