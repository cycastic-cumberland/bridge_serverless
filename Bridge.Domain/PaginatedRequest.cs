namespace Bridge.Domain;

public readonly struct PaginatedRequest
{
    public required int PageNumber { get; init; }
    
    public required int ItemPerPage { get; init; }
}