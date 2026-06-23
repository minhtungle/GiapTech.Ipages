using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Media.Commands;
using GiapTech.Ipages.Application.Media.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IStorageService _storage;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUser;

    public MediaController(ISender sender, IStorageService storage, IApplicationDbContext db, ICurrentTenantService tenantService, ICurrentUserService currentUser)
    {
        _sender = sender;
        _storage = storage;
        _db = db;
        _tenantService = tenantService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 30, [FromQuery] string? folder = null, [FromQuery] MediaFileType? type = null)
    {
        var result = await _sender.Send(new GetMediaFilesQuery(page, pageSize, folder, type));
        return Ok(result);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string? folder = null, [FromForm] string? altText = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Không có file được upload.");

        if (_tenantService.TenantId == null)
            throw new ForbiddenException();

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var storagePath = string.IsNullOrWhiteSpace(folder)
            ? $"media/{_tenantService.TenantId}/{fileName}"
            : $"media/{_tenantService.TenantId}/{folder}/{fileName}";

        using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(storagePath, stream, file.ContentType);

        var mediaType = file.ContentType.StartsWith("image/") ? MediaFileType.Image
            : file.ContentType.StartsWith("video/") ? MediaFileType.Video
            : extension is ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" ? MediaFileType.Document
            : MediaFileType.Other;

        var mediaFile = new MediaFile
        {
            TenantId = _tenantService.TenantId.Value,
            FileName = fileName,
            OriginalName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            Url = url,
            StoragePath = storagePath,
            Type = mediaType,
            Folder = folder,
            AltText = altText,
            UploadedBy = _currentUser.UserId ?? Guid.Empty
        };

        _db.MediaFiles.Add(mediaFile);
        await _db.SaveChangesAsync(default);

        return Ok(new { mediaFile.Id, mediaFile.Url, mediaFile.FileName, mediaFile.OriginalName, mediaFile.ContentType, mediaFile.FileSize, mediaFile.Type });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteMediaFileCommand(id));
        return NoContent();
    }
}
