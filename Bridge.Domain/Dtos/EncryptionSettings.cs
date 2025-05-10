namespace Bridge.Domain.Dtos;

public class EncryptionSettings
{
    public required string Key { get; set; }

    public EncryptionSettings Base64UrlEncode()
    {
        Span<char> original = stackalloc char[Key.Length + 2];
        var span = original[..Key.Length];
        Key.CopyTo(span);

        span = span.TrimEnd('=');
        span.Replace('_', '/');
        span.Replace('-', '+');
        switch (span.Length)
        {
            case 2:
            {
                span = original[..(span.Length + 2)];
                span[^2] = '=';
                span[^1] = '=';
                break;
            }
            case 3:
            {
                span = original[..(span.Length + 1)];
                span[^1] = '=';
                break;
            }
        }

        return new()
        {
            Key = new(span)
        };
    }
}