namespace Bridge.Domain.Configurations;

public class ItemConfigurations
{
    public uint? ItemExpirationMinutes { get; set; }
    
    public uint? PreSignedUploadUrlExpirationMinutes { get; set; }
    
    public uint? PreSignDownloadUrlExpirationMinutes { get; set; }
}