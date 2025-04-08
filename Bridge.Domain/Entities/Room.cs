using System.Text.Json.Serialization;

namespace Bridge.Domain.Entities;

public class Room
{
    public const long DefaultSortKey = 1;
    
    public required Guid Id { get; set; }
    
    [JsonPropertyName("pk")]
    public string PartitionKey => this.CreatePartitionKey(Id.ToString());

    [JsonPropertyName("sk")]
    public long SortKey => DefaultSortKey;

    [JsonPropertyName("ttl")]
    public long TimeToLive => ExpiredAt.ToUniversalTime().ToUnixTimeSeconds();
    
    public required DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset ExpiredAt { get; set; }
}