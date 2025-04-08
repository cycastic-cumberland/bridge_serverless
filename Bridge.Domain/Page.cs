namespace Bridge.Domain;

public readonly struct Page<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }
    
    public required int PageNumber { get; init; }
    
    public required int TotalSize { get; init; }
}