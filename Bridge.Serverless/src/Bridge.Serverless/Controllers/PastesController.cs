using Bridge.Domain;
using Bridge.Domain.Dtos;
using Bridge.Domain.Exceptions;
using Bridge.Infrastructure.Abstractions;
using Bridge.Serverless.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Bridge.Serverless.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PastesController : ControllerBase
{
    private readonly IPasteRepository _pasteRepository;

    public PastesController(IPasteRepository pasteRepository)
    {
        _pasteRepository = pasteRepository;
    }

    [HttpGet("pastes")]
    [ProducesResponseType(typeof(Page<PasteDto>), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<Page<PasteDto>> Query(Guid roomId,
        int pageNumber = 1,
        int itemPerPage = 5,
        CancellationToken cancellationToken = default)
    {
        return _pasteRepository.GetLatestPastesAsync(roomId,
            new()
            {
                PageNumber = pageNumber,
                ItemPerPage = itemPerPage,
            }, cancellationToken);
    }

    [HttpGet("paste")]
    [ProducesResponseType(typeof(PasteDto), 200)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task<PasteDto> GetPaste(Guid roomId, long pasteId, bool truncate, CancellationToken cancellationToken)
    {
        return _pasteRepository.GetPasteAsync(roomId, pasteId, truncate, cancellationToken);
    }

    [HttpPut]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ExceptionDto), 400)]
    [ProducesResponseType(typeof(ExceptionDto), 404)]
    public Task CreatePaste(Guid roomId, CreatePasteDto request, CancellationToken cancellationToken)
    {
        return _pasteRepository.CreatePasteAsync(roomId, request.Content, cancellationToken);
    }
}