namespace Bridge.Domain;

public static class ConfigurationHelpers
{
    public static string GetEnvironmentVariable(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Environment.GetEnvironmentVariable(name) ??
               throw new InvalidOperationException($"Environment variable {name} was not set");
    }
    
    public static string GetEnvironmentVariable(string name, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(defaultValue);
        return Environment.GetEnvironmentVariable(name) ??defaultValue;
    }
}