using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Media.Queries;

public record MediaFileDto(
    Guid Id,
    string FileName,
    string OriginalName,
    string ContentType,
    long FileSize,
    string Url,
    MediaFileType Type,
    string? Folder,
    string? AltText,
    int? Width,
    int? Height,
    DateTime CreatedAt);

public record GetMediaFilesQuery(
    int Page = 1,
    int PageSize = 30,
    string? Folder = null,
    MediaFileType? Type = null) : IRequest<PaginatedList<MediaFileDto>>;

public class GetMediaFilesQueryHandler : IRequestHandler<GetMediaFilesQuery, PaginatedList<MediaFileDto>>
{
    private readonly IApplicationDbContext _db;

    public GetMediaFilesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<MediaFileDto>> Handle(GetMediaFilesQuery request, CancellationToken ct)
    {
        IQueryable<MediaFile> query = _db.MediaFiles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Folder))
            query = query.Where(m => m.Folder == request.Folder);

        if (request.Type.HasValue)
            query = query.Where(m => m.Type == request.Type.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MediaFileDto(m.Id, m.FileName, m.OriginalName, m.ContentType, m.FileSize, m.Url, m.Type, m.Folder, m.AltText, m.Width, m.Height, m.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<MediaFileDto>(items, total, request.Page, request.PageSize);
    }
}
