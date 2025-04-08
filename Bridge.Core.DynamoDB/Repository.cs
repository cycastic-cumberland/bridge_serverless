using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using InternalServerErrorException = Bridge.Domain.Exceptions.InternalServerErrorException;

namespace Bridge.Core.DynamoDB;

public abstract class Repository<TConfig> where TConfig : class
{
    private readonly IOptions<TConfig> _config;
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly ILogger _logger;

    public TConfig Configurations => _config.Value;

    protected IAmazonDynamoDB DynamoDb => _dynamoDb;

    protected static string TableName => Environment.GetEnvironmentVariable("AWS_DYNAMODB_TABLE") ??
                                          throw new InvalidOperationException("AWS_DYNAMODB_TABLE was not supplied");
    
    protected Repository(IOptions<TConfig> configurations, IAmazonDynamoDB dynamoDb, ILogger logger)
    {
        _config = configurations;
        _dynamoDb = dynamoDb;
        _logger = logger;
    }

    protected async Task PutAsync<T>(T instance, CancellationToken cancellationToken) where T : class
    {
        var instanceAsJson = JsonSerializer.Serialize(instance);
        var instanceAsDocument = Document.FromJson(instanceAsJson);
        var asAttributes = instanceAsDocument.ToAttributeMap();
        var putRequest = new PutItemRequest
        {
            TableName = TableName,
            Item = asAttributes
        };

        var response = await _dynamoDb.PutItemAsync(putRequest, cancellationToken);
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Failed to put object: {Response}", response);
            throw new InternalServerErrorException("Failed to put object");
        }
    }
    
    protected async Task<List<T>> GetAllItemsDescendingPaginated<T>(
        string partitionKeyValue,
        int pageSize,
        CancellationToken cancellationToken) 
        where T : class
    {
        var request = new QueryRequest
        {
            TableName = TableName,
            KeyConditionExpression = "pk = :pkVal",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pkVal", new AttributeValue { S = partitionKeyValue } }
            },
            ScanIndexForward = false,
            Limit = pageSize
        };

        var list = new List<T>();
        do
        {
            var response = await _dynamoDb.QueryAsync(request, cancellationToken);
            foreach (var item in response.Items)
            {
                var doc = Document.FromAttributeMap(item);
                var instance = JsonSerializer.Deserialize<T>(doc.ToJson())
                               ?? throw new InternalServerErrorException("Failed to deserialize document");
                list.Add(instance);

                if (list.Count == pageSize)
                {
                    return list;
                }
            }
            // Set ExclusiveStartKey for the next page, if any:
            request.ExclusiveStartKey = response.LastEvaluatedKey;
        }
        while (request.ExclusiveStartKey is { Count: > 0 });

        return list;
    }
}