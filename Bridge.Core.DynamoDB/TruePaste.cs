using System.Text.Json.Serialization;
using Bridge.Domain;

namespace Bridge.Core.DynamoDB;

internal class TruePaste
{
    private static DateTimeOffset Epoch { get; } = DateTimeOffset.Parse("1970-01-01T00:00:00.000Z");
    
    [JsonPropertyName("pk")]
    public string PartitionKey => this.CreatePartitionKey(RoomId.ToString());
    
    [JsonPropertyName("sk")]
    public long SortKey => (long)(CreatedAt - Epoch).TotalMicroseconds;
    
    [JsonPropertyName("ttl")]
    public long TimeToLive => ExpiredAt.ToUniversalTime().ToUnixTimeSeconds();
    
    public required Guid RoomId { get; set; }
    
    public required string Content { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiredAt { get; set; }
}