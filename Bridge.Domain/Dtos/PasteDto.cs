namespace Bridge.Domain.Dtos;

public class PasteDto
{
    public long Id { get; set; }
    
    public Guid RoomId { get; set; }
    
    public required string Content { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset ExpiredAt { get; set; }
}