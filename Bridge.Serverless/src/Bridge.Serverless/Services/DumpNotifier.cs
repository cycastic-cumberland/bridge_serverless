using Bridge.Infrastructure.Abstractions;

namespace Bridge.Serverless.Services;

public class DumpNotifier : INotifier
{
    public static DumpNotifier Default { get; } = new();
    
    public Task NotifyAsync<T>(string channelName, string eventName, T eventPayload,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
