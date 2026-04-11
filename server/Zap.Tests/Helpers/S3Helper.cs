using Amazon.S3;
using Amazon.S3.Model;

namespace Zap.Tests.Helpers;

internal static class S3BucketTestHelper
{
    internal static async Task ClearTestBucketAsync(this IAmazonS3 s3Client, string bucketName)
    {
        EnsureSafeTestBucketName(bucketName);

        try
        {
            var objects = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucketName
            });

            if (objects == null || objects.S3Objects.Count == 0) return;

            var res = await s3Client.DeleteObjectsAsync(new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = objects.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList()
            });

            Assert.NotNull(res);
            Assert.Empty(res.DeleteErrors);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw new Exception("Failed to delete objects");
        }
    }

    internal static void EnsureSafeTestBucketName(string bucketName)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
            throw new InvalidOperationException("S3 integration tests require AWS_S3_BUCKET to be set.");

        if (!bucketName.Contains("test", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Refusing to run S3 integration tests against non-test bucket '{bucketName}'. Configure server/Zap.Tests/.env.test with a dedicated test bucket.");
    }
}
