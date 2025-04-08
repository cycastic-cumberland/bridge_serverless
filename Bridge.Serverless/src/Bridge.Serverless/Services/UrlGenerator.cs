using Bridge.Infrastructure.Abstractions;
using Bridge.Serverless.Dto;
using Microsoft.Extensions.Options;

namespace Bridge.Serverless.Services;

public class UrlGenerator : IUrlGenerator
{
    private readonly Uri _basePath;

    public UrlGenerator(IOptions<AppSettings> options)
    {
        _basePath = new(options.Value.FrontendUrl);
    }

    public string GetFrontendRoomUrl(Guid roomId)
    {
        return new Uri(_basePath, roomId.ToString()).ToString();
    }
}