using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Bridge.Domain;
using Bridge.Domain.Configurations;
using Bridge.Domain.Dtos;
using Bridge.Domain.Entities;
using Bridge.Domain.Exceptions;
using Bridge.Infrastructure.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InternalServerErrorException = Bridge.Domain.Exceptions.InternalServerErrorException;

namespace Bridge.Core.DynamoDB;

public class ItemRepository : Repository<ItemConfigurations>, IItemRepository
{
    private readonly RoomRepository _roomRepository;
    private readonly IStorageService _storageService;
    private readonly INotifier _notifier;
    
    public ItemRepository(IOptions<ItemConfigurations> configurations,
        IAmazonDynamoDB dynamoDb,
        ILogger<ItemRepository> logger,
        RoomRepository roomRepository,
        IStorageService storageService,
        INotifier notifier)
        : base(configurations, dynamoDb, logger)
    {
        _roomRepository = roomRepository;
        _storageService = storageService;
        _notifier = notifier;
    }

    public async Task<UploadPreSignedDto> GetPreSignedUploadUrlAsync(Guid roomId, string fileName, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomAsync(roomId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        room.ExpiredAt = now.AddMinutes(_roomRepository.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        var extension = Path.GetExtension(fileName);
        var item = new Item
        {
            RoomId = room.Id,
            FileName = fileName,
            StorageKey = $"{Guid.NewGuid()}{extension}",
            CreatedAt = now,
            ExpiredAt = now.AddMinutes(Configurations.ItemExpirationMinutes ?? 10),
        };

        await _roomRepository.PutRoomAsync(room, cancellationToken);
        await PutAsync(item, cancellationToken);
        var url = await _storageService.GetPreSignedUrlAsync(item.StorageKey,
            item.FileName,
            true,
            DateTimeOffset.UtcNow.AddMinutes(Configurations.PreSignedUploadUrlExpirationMinutes ?? 5));
        return new(item.SortKey, url);
    }

    public async Task<Page<ItemDto>> GetLatestItemsAsync(Guid roomId, PaginatedRequest request, CancellationToken cancellationToken)
    {
        if (request.PageNumber != 1)
        {
            throw new InternalServerErrorException("Page numbers other than 1 are not supported");
        }
        var room = await _roomRepository.GetRoomAsync(roomId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        room.ExpiredAt = now.AddMinutes(_roomRepository.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        await _roomRepository.PutRoomAsync(room, cancellationToken);

        var items = await GetAllItemsDescendingPaginated<Item>(EntitiesHelpers.CreatePartitionKey<Item>(room.Id.ToString()),
            request.ItemPerPage,
            cancellationToken);
        return new()
        {
            Items = items.Select(static i => new ItemDto
                {
                    Id = i.SortKey,
                    RoomId = i.RoomId,
                    FileName = i.FileName,
                    CreatedAt = i.CreatedAt,
                    ExpiredAt = i.ExpiredAt
                })
                .ToList(),
            PageNumber = request.PageNumber,
            TotalSize = items.Count, // Unknown
        };
    }

    private async Task<Item> GetItemAsync(Guid roomId, long itemId, CancellationToken cancellationToken)
    {
        var pk = EntitiesHelpers.CreatePartitionKey<Item>(roomId.ToString());
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["pk"] = new() { S = pk },
                ["sk"] = new() { N = itemId.ToString()},
            }
        };

        var response = await DynamoDb.GetItemAsync(request, cancellationToken);
        if (response.Item.Count == 0)
        {
            throw new NotFoundException($"Could not found item with ID {roomId}");
        }

        var itemAsDocument = Document.FromAttributeMap(response.Item);
        return JsonSerializer.Deserialize<Item>(itemAsDocument.ToJson()) ??
               throw new InternalServerErrorException("Failed to deserialize document");
    }

    public async Task MakeReadyAsync(Guid roomId, long itemId, CancellationToken cancellationToken)
    {
        var item = await GetItemAsync(roomId, itemId, cancellationToken);
        if (item.IsReady)
        {
            return;
        }

        item.IsReady = true;
        await PutAsync(item, cancellationToken);
        await _notifier.NotifyAsync(roomId.ToString(),
            NotificationPayload.Items.UpdatedObjects,
            NotificationPayload.Items,
            cancellationToken);
    }

    public async Task<string> GetPreSignedDownloadUrlAsync(Guid roomId, long itemId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var item = await GetItemAsync(roomId, itemId, cancellationToken);
        item.ExpiredAt = now.AddMinutes(Configurations.ItemExpirationMinutes ?? 10);
        await PutAsync(item, cancellationToken);
        
        var room = await _roomRepository.GetRoomAsync(roomId, cancellationToken);
        room.ExpiredAt = now.AddMinutes(_roomRepository.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        await PutAsync(room, cancellationToken);
        
        return await _storageService.GetPreSignedUrlAsync(item.StorageKey, item.FileName, false,
            now.AddMinutes(Configurations.PreSignDownloadUrlExpirationMinutes ?? 5));
    }
}