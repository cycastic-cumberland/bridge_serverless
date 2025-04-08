using System.Text;

namespace Bridge.Infrastructure.Abstractions;

public static class StringExtensions
{
    public static string Truncate(this string? value, int maxLength, string truncationSuffix, out bool truncated)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 1);
        if (value == null)
        {
            truncated = false;
            return string.Empty;
        }

        if (value.Length > maxLength)
        {
            truncated = true;
            return new StringBuilder().Append(value.AsSpan()[..(maxLength - 1)]).Append(truncationSuffix).ToString();
        }

        truncated = false;
        return value;
    }
    
    public static string Truncate(this string? value, int maxLength, out bool truncated)
    {
        return value.Truncate(maxLength, "…", out truncated);
    }
}