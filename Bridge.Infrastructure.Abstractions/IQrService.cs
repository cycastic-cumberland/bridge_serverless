namespace Bridge.Infrastructure.Abstractions;

public interface IQrService
{
    byte[] GenerateSvgQrCode(string plainText);
}