namespace Bridge.Infrastructure.Abstractions;

public interface INotifier
{
    Task NotifyAsync<T>(string channelName, string eventName, T eventPayload, CancellationToken cancellationToken);
}