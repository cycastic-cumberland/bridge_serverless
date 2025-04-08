namespace Bridge.Domain;

public static class EntitiesHelpers
{
    public static string CreatePartitionKey<T>(string subKey) where T : class
    {
        return $"{typeof(T).Name}/{subKey}";
    }

    public static string CreatePartitionKey<T>(this T _, string subKey) where T : class
        => CreatePartitionKey<T>(subKey);
}