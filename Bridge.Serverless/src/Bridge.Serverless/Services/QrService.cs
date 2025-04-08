using System.Text;
using Bridge.Domain.Configurations;
using Bridge.Infrastructure.Abstractions;
using Microsoft.Extensions.Options;
using QRCoder;

namespace Bridge.Serverless.Services;

public class QrService : IQrService
{
    public byte[] GenerateSvgQrCode(string plainText)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(plainText, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new SvgQRCode(data);
        return Encoding.UTF8.GetBytes(qrCode.GetGraphic((int)(20U)));
    }
}