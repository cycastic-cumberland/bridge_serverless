namespace Bridge.Serverless.Dto;

public class RoomCreationDto
{
    public required Guid RoomId { get; set; }
    
    public required string EncryptionKey { get; set; }
}