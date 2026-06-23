using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.LandingPages.Queries;

public record LandingSectionDto(Guid Id, string Type, string? Title, string? Settings, int SortOrder, bool IsVisible);

public record LandingPageListDto(Guid Id, string Name, string Slug, string? Template, LandingPageStatus Status, DateTime? PublishedAt, DateTime CreatedAt);

public record LandingPageDetailDto(Guid Id, string Name, string Slug, string? Template, LandingPageStatus Status, string? MetaTitle, string? MetaDescription, string? OgImage, DateTime? PublishedAt, IEnumerable<LandingSectionDto> Sections, DateTime CreatedAt);

public record GetLandingPagesQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedList<LandingPageListDto>>;

public class GetLandingPagesQueryHandler : IRequestHandler<GetLandingPagesQuery, PaginatedList<LandingPageListDto>>
{
    private readonly IApplicationDbContext _db;

    public GetLandingPagesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<LandingPageListDto>> Handle(GetLandingPagesQuery request, CancellationToken ct)
    {
        var total = await _db.LandingPages.CountAsync(ct);
        var items = await _db.LandingPages.AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new LandingPageListDto(p.Id, p.Name, p.Slug, p.Template, p.Status, p.PublishedAt, p.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<LandingPageListDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetLandingPageBySlugQuery(string Slug) : IRequest<LandingPageDetailDto>;

public class GetLandingPageBySlugQueryHandler : IRequestHandler<GetLandingPageBySlugQuery, LandingPageDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetLandingPageBySlugQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<LandingPageDetailDto> Handle(GetLandingPageBySlugQuery request, CancellationToken ct)
    {
        var p = await _db.LandingPages.AsNoTracking()
            .Include(x => x.Sections)
            .FirstOrDefaultAsync(x => x.Slug == request.Slug, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(LandingPage), request.Slug);

        return new LandingPageDetailDto(p.Id, p.Name, p.Slug, p.Template, p.Status, p.MetaTitle, p.MetaDescription, p.OgImage, p.PublishedAt,
            p.Sections.OrderBy(s => s.SortOrder).Select(s => new LandingSectionDto(s.Id, s.Type, s.Title, s.Settings, s.SortOrder, s.IsVisible)),
            p.CreatedAt);
    }
}
