namespace Bridge.Core.DynamoDB;

internal class NotificationPayload
{
    public required string UpdatedObjects { get; set; }

    public static NotificationPayload Items { get; } = new() { UpdatedObjects = "items" };
    
    public static NotificationPayload Pastes { get; } = new() { UpdatedObjects = "pastes" };
}