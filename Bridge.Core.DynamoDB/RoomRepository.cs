using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Bridge.Domain;
using Bridge.Domain.Configurations;
using Bridge.Domain.Entities;
using Bridge.Domain.Exceptions;
using Bridge.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InternalServerErrorException = Bridge.Domain.Exceptions.InternalServerErrorException;

namespace Bridge.Core.DynamoDB;

public class RoomRepository : Repository<RoomConfigurations>, IRoomRepository
{
    private static AttributeValue DefaultSortKey { get; } = new() { N = Room.DefaultSortKey.ToString() };
    
    public RoomRepository(IOptions<RoomConfigurations> configurations,
        IAmazonDynamoDB dynamoDb,
        ILogger<RoomRepository> logger)
        : base(configurations, dynamoDb, logger)
    {
    }

    internal Task PutRoomAsync(Room room, CancellationToken cancellationToken)
    {
        return PutAsync(room, cancellationToken);
    }

    public async Task<Guid> CreateRoomAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var id = Guid.NewGuid();
        var room = new Room
        {
            Id = id,
            CreatedAt = now,
            ExpiredAt = now.AddMinutes(Configurations.RoomCreationExpirationMinutes ?? 60)
        };

        await PutRoomAsync(room, cancellationToken);
        return id;
    }

    private async Task<GetItemResponse> GetRoomItemAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["pk"] = new() { S = EntitiesHelpers.CreatePartitionKey<Room>(roomId.ToString()) },
                ["sk"] = DefaultSortKey,
            }
        };

        var response = await DynamoDb.GetItemAsync(request, cancellationToken);
        return response;
    }

    public async Task<bool> ExistsAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var response = await GetRoomItemAsync(roomId, cancellationToken);
        return response.Item.Count > 0;
    }

    internal async Task<Room> GetRoomAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var response = await GetRoomItemAsync(roomId, cancellationToken);
        if (response.Item.Count == 0)
        {
            throw new NotFoundException($"Could not found room with ID {roomId}");
        }
        var itemAsDocument = Document.FromAttributeMap(response.Item);
        var json = itemAsDocument.ToJson();
        return JsonSerializer.Deserialize<Room>(json) ??
               throw new InternalServerErrorException("Failed to deserialize document");
    }
}