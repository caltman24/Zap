using Amazon.S3;
using Amazon.S3.Model;
using dotenv.net.Utilities;

namespace Zap.Tests.Helpers;

internal static class S3Helper
{
    internal static async Task ClearTestBucketAsync(this IAmazonS3 s3Client)
    {
        try
        {
            var bucketName = EnvReader.GetStringValue("AWS_S3_BUCKET");
            var objects = await s3Client.ListObjectsV2Async(new ListObjectsV2Request()
            {
                BucketName = bucketName
            });

            if (objects == null || objects.S3Objects.Count == 0) return;

            var res = await s3Client.DeleteObjectsAsync(new DeleteObjectsRequest()
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
}