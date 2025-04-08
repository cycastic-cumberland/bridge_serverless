namespace Bridge.Infrastructure.Abstractions;

public interface IRoomRepository
{
    Task<Guid> CreateRoomAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid roomId, CancellationToken cancellationToken);
}