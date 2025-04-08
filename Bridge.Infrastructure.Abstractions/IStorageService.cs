namespace Bridge.Infrastructure.Abstractions;

public interface IStorageService
{
    Task UploadAsync(string key, Stream stream, CancellationToken cancellationToken);

    Task<string> GetPreSignedUrlAsync(string key, string fileName, bool isUpload, DateTimeOffset expiredAt);
}