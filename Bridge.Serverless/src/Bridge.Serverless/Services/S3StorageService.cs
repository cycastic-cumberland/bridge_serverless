using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Bridge.Infrastructure.Abstractions;

namespace Bridge.Serverless.Services;

public class S3StorageService : IStorageService, IDisposable
{
     private readonly AmazonS3Client _s3Client;
     
     protected static string BucketName => Environment.GetEnvironmentVariable("AWS_S3_BUCKET") ??
                                           throw new InvalidOperationException("AWS_S3_BUCKET was not supplied");

    public S3StorageService()
    {
        _s3Client = new();
    }
    
    public async Task UploadAsync(string key, Stream stream, CancellationToken cancellationToken)
    {
        using var transferUtility = new TransferUtility(_s3Client);

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            BucketName = BucketName,
            Key = key,
        };

        await transferUtility.UploadAsync(uploadRequest, cancellationToken);
    }

    public Task<string> GetPreSignedUrlAsync(string key, string fileName, bool isUpload, DateTimeOffset expiredAt)
    {
        var preSignedUrlRequest = new GetPreSignedUrlRequest
        {
            BucketName = BucketName,
            Key = key,
            Verb = isUpload ? HttpVerb.PUT : HttpVerb.GET,
            Expires = expiredAt.UtcDateTime,
            Protocol = Protocol.HTTPS
        };
        if (isUpload)
        {
            preSignedUrlRequest.ContentType = "application/octet-stream";
        }
        else
        {
            preSignedUrlRequest.ResponseHeaderOverrides = new ResponseHeaderOverrides
            {
                ContentDisposition = $"attachment; filename=\"{fileName}\""
            };
        }

        return _s3Client.GetPreSignedURLAsync(preSignedUrlRequest);
    }

    public void Dispose()
    {
        _s3Client.Dispose();
    }
}