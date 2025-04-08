using Bridge.Domain;
using Bridge.Domain.Dtos;

namespace Bridge.Infrastructure.Abstractions;

public interface IItemRepository
{
    Task<UploadPreSignedDto> GetPreSignedUploadUrlAsync(Guid roomId,
        string fileName,
        CancellationToken cancellationToken);

    Task<Page<ItemDto>> GetLatestItemsAsync(Guid roomId, PaginatedRequest request, CancellationToken cancellationToken);

    Task MakeReadyAsync(Guid roomId, long itemId, CancellationToken cancellationToken);

    Task<string> GetPreSignedDownloadUrlAsync(Guid roomId, long itemId, CancellationToken cancellationToken);
}