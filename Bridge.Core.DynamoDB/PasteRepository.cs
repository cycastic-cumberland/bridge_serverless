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

public class PasteRepository : Repository<PasteConfigurations>, IPasteRepository
{
    private readonly RoomRepository _roomRepository;
    private readonly INotifier _notifier;

    public PasteRepository(RoomRepository roomRepository,
        IOptions<PasteConfigurations> configurations,
        IAmazonDynamoDB dynamoDb,
        ILogger<PasteRepository> logger,
        INotifier notifier)
        : base(configurations, dynamoDb, logger)
    {
        _roomRepository = roomRepository;
        _notifier = notifier;
    }

    public async Task CreatePasteAsync(Guid roomId, string content, CancellationToken cancellationToken)
    {
        if (content.Length > (Configurations.LengthLimit ?? 8192))
        {
            throw new BadRequestException("Content length is over the preset limit.");
        }
        var room = await _roomRepository.GetRoomAsync(roomId, cancellationToken);
        var truncatedContent = content.Truncate(Paste.TruncatedLength, out var truncated);
        var now = DateTimeOffset.UtcNow;
        var paste = new Paste
        {
            RoomId = room.Id,
            TruncatedContent = truncatedContent,
            Truncated = truncated,
            CreatedAt = now,
            ExpiredAt = now.AddMinutes(Configurations.PasteExpirationMinutes ?? 5)
        };

        await PutAsync(paste, cancellationToken);

        if (truncated)
        {
            var truePaste = new TruePaste
            {
                RoomId = room.Id,
                Content = truncated ? content : string.Empty,
                CreatedAt = now,
                ExpiredAt = now.AddMinutes(Configurations.PasteExpirationMinutes ?? 5)
            };
            await PutAsync(truePaste, cancellationToken);
        }

        await _notifier.NotifyAsync(roomId.ToString(),
            NotificationPayload.Pastes.UpdatedObjects,
            NotificationPayload.Pastes,
            cancellationToken);
    }
    
    private async Task<T> GetPasteAsync<T>(Guid roomId, long itemId, CancellationToken cancellationToken)
        where T : class
    {
        var pk = EntitiesHelpers.CreatePartitionKey<T>(roomId.ToString());
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
        return JsonSerializer.Deserialize<T>(itemAsDocument.ToJson()) ??
               throw new InternalServerErrorException("Failed to deserialize document");
    }

    public async Task<PasteDto> GetPasteAsync(Guid roomId, long pasteId, bool truncate, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomAsync(roomId, cancellationToken);
        room.ExpiredAt = room.ExpiredAt
            .AddMinutes(_roomRepository.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        await _roomRepository.PutRoomAsync(room, cancellationToken);

        var paste = await GetPasteAsync<Paste>(roomId, pasteId, cancellationToken);
        if (truncate)
        {
            return new()
            {
                Id = paste.SortKey,
                RoomId = roomId,
                Content = paste.TruncatedContent,
                CreatedAt = paste.CreatedAt,
                ExpiredAt = paste.ExpiredAt
            };
        }

        if (!paste.Truncated)
        {
            return new()
            {
                Id = paste.SortKey,
                RoomId = roomId,
                Content = paste.TruncatedContent,
                CreatedAt = paste.CreatedAt,
                ExpiredAt = paste.ExpiredAt
            };
        }

        var truePaste = await GetPasteAsync<TruePaste>(roomId, pasteId, cancellationToken);
        return new()
        {
            Id = truePaste.SortKey,
            RoomId = roomId,
            Content = truePaste.Content,
            CreatedAt = truePaste.CreatedAt,
            ExpiredAt = truePaste.ExpiredAt
        };
    }

    public async Task<Page<PasteDto>> GetLatestPastesAsync(Guid roomId, PaginatedRequest request, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomAsync(roomId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        room.ExpiredAt = now.AddMinutes(_roomRepository.Configurations.RoomResurrectionExpirationMinutes ?? 120);
        await _roomRepository.PutRoomAsync(room, cancellationToken);

        var pastes = await GetAllItemsDescendingPaginated<Paste>(EntitiesHelpers.CreatePartitionKey<Paste>(room.Id.ToString()),
            request.ItemPerPage,
            cancellationToken);

        return new()
        {
            Items = pastes.Select(static p => new PasteDto
                {
                    Id = p.SortKey,
                    RoomId = p.RoomId,
                    Content = p.TruncatedContent,
                    CreatedAt = p.CreatedAt,
                    ExpiredAt = p.ExpiredAt
                })
                .ToList(),
            PageNumber = request.PageNumber,
            TotalSize = pastes.Count, // Unknown
        };
    }
}