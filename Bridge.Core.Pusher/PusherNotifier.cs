using Bridge.Domain;
using Bridge.Infrastructure.Abstractions;
using PusherServer;

namespace Bridge.Core.Pusher;

public class PusherNotifier : INotifier
{
    public const string AppIdConstant = "PUSHER_ID";
    
    private readonly PusherServer.Pusher _client;

    protected PusherNotifier(string cluster, string appId, string appKey, string secretKey)
    {
        var options = new PusherOptions()
        {
            Cluster = cluster,
            Encrypted = true
        };

        _client = new PusherServer.Pusher(appId, appKey, secretKey, options);
    }

    public PusherNotifier()
        : this(ConfigurationHelpers.GetEnvironmentVariable("PUSHER_CLUSTER"),
            ConfigurationHelpers.GetEnvironmentVariable(AppIdConstant),
            ConfigurationHelpers.GetEnvironmentVariable("PUSHER_KEY"),
            ConfigurationHelpers.GetEnvironmentVariable("PUSHER_SECRET"))
    {
    }

    public Task NotifyAsync<T>(string channelName, string eventName, T eventPayload,
        CancellationToken cancellationToken)
    {
        return _client.TriggerAsync(channelName, eventName, eventPayload);
    }
}