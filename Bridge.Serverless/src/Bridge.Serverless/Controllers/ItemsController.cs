using Bridge.Domain;
using Bridge.Domain.Dtos;
using Bridge.Infrastructure.Abstractions;
using Bridge.Serverless.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Serverless.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    private readonly IItemRepository _itemService;
    private readonly IQrService _qrService;

    public ItemsController(IItemRepository itemService, IQrService qrService)
    {
        _itemService = itemService;
        _qrService = qrService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Page<ItemDto>), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<Page<ItemDto>> Query(Guid roomId,
        int pageNumber = 1,
        int itemPerPage = 5,
        CancellationToken cancellationToken = default)
    {
        return _itemService.GetLatestItemsAsync(roomId,
            new()
            {
                PageNumber = pageNumber,
                ItemPerPage = itemPerPage
            },
            cancellationToken);
    }

    [HttpGet("upload-presigned")]
    [ProducesResponseType(typeof(UploadPreSignedDto), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<UploadPreSignedDto> GetPreSignedUploadUrl(Guid roomId,
        string fileName,
        CancellationToken cancellationToken)
    {
        return _itemService.GetPreSignedUploadUrlAsync(roomId, fileName, cancellationToken);
    }

    [HttpPost("ready")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task MakeReady(Guid roomId, long itemId, CancellationToken cancellationToken)
    {
        return _itemService.MakeReadyAsync(roomId, itemId, cancellationToken);
    }

    [HttpGet("download-presigned")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<string> GetPreSignedDownloadUrl(Guid roomId,
        long itemId,
        CancellationToken cancellationToken)
    {
        return _itemService.GetPreSignedDownloadUrlAsync(roomId, itemId, cancellationToken);
    }
    
    [HttpGet("redirect-download-presigned")]
    [ProducesResponseType(302)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public async Task<RedirectResult> RedirectPreSignedDownloadUrl(Guid roomId,
        long itemId,
        CancellationToken cancellationToken)
    {
        var url = await _itemService.GetPreSignedDownloadUrlAsync(roomId, itemId, cancellationToken);
        return Redirect(url);
    }

    [HttpGet("redirect-download-presigned-qr")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 500)]
    [ResponseCache(Duration = int.MaxValue)]
    public FileContentResult GetRedirectPreSignedDownloadQr(Guid roomId, long itemId)
    {
        var url = Url.Action(nameof(RedirectPreSignedDownloadUrl), new
        {
            roomId, itemId
        }) ?? throw new InvalidOperationException("Could not generate QR code");
        url = Request.Host + url;
        var png = _qrService.GenerateSvgQrCode(url);
        return File(png, "image/png");
    }
}