using Bridge.Domain;
using Bridge.Domain.Dtos;

namespace Bridge.Infrastructure.Abstractions;

public interface IPasteRepository
{
    Task CreatePasteAsync(Guid roomId, string content, CancellationToken cancellationToken);

    Task<PasteDto> GetPasteAsync(Guid roomId, long pasteId, bool truncate, CancellationToken cancellationToken);

    Task<Page<PasteDto>> GetLatestPastesAsync(Guid roomId,
        PaginatedRequest request,
        CancellationToken cancellationToken);
}