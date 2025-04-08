namespace Bridge.Domain.Dtos;

public class ItemDto
{
    public long Id { get; set; }
    
    public required Guid RoomId { get; set; }
    
    public required string FileName { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset ExpiredAt { get; set; }
}