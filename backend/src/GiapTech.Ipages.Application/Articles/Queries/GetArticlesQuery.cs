using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Articles.Queries;

public record ArticleListDto(
    Guid Id,
    string Title,
    string Slug,
    string? Excerpt,
    string? ThumbnailUrl,
    Guid? CategoryId,
    string? CategoryName,
    ArticleStatus Status,
    DateTime? PublishedAt,
    int ViewCount,
    DateTime CreatedAt);

public record ArticleDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string? Excerpt,
    string Content,
    string? ThumbnailUrl,
    Guid? CategoryId,
    string? CategoryName,
    Guid AuthorId,
    ArticleStatus Status,
    DateTime? PublishedAt,
    DateTime? ScheduledAt,
    int ViewCount,
    string? MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    string? OgImage,
    DateTime CreatedAt);

public record GetArticlesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? CategoryId = null,
    ArticleStatus? Status = null,
    bool PublishedOnly = false) : IRequest<PaginatedList<ArticleListDto>>;

public class GetArticlesQueryHandler : IRequestHandler<GetArticlesQuery, PaginatedList<ArticleListDto>>
{
    private readonly IApplicationDbContext _db;

    public GetArticlesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<ArticleListDto>> Handle(GetArticlesQuery request, CancellationToken ct)
    {
        var query = _db.Articles.AsNoTracking().Include(a => a.Category);

        IQueryable<Article> filtered = query;

        if (!string.IsNullOrWhiteSpace(request.Search))
            filtered = filtered.Where(a => a.Title.Contains(request.Search) || a.Slug.Contains(request.Search));

        if (request.CategoryId.HasValue)
            filtered = filtered.Where(a => a.CategoryId == request.CategoryId.Value);

        if (request.Status.HasValue)
            filtered = filtered.Where(a => a.Status == request.Status.Value);

        if (request.PublishedOnly)
            filtered = filtered.Where(a => a.Status == ArticleStatus.Published);

        var total = await filtered.CountAsync(ct);
        var items = await filtered
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new ArticleListDto(a.Id, a.Title, a.Slug, a.Excerpt, a.ThumbnailUrl, a.CategoryId, a.Category != null ? a.Category.Name : null, a.Status, a.PublishedAt, a.ViewCount, a.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<ArticleListDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetArticleByIdQuery(Guid Id) : IRequest<ArticleDetailDto>;

public class GetArticleByIdQueryHandler : IRequestHandler<GetArticleByIdQuery, ArticleDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetArticleByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ArticleDetailDto> Handle(GetArticleByIdQuery request, CancellationToken ct)
    {
        var a = await _db.Articles.AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Article), request.Id);

        return MapToDetail(a);
    }
}

public record GetArticleBySlugQuery(string Slug) : IRequest<ArticleDetailDto>, ICacheable
{
    public string CacheKey => $"article:slug:{Slug}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}

public class GetArticleBySlugQueryHandler : IRequestHandler<GetArticleBySlugQuery, ArticleDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetArticleBySlugQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ArticleDetailDto> Handle(GetArticleBySlugQuery request, CancellationToken ct)
    {
        var a = await _db.Articles.AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Slug == request.Slug && x.Status == ArticleStatus.Published, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Article), request.Slug);

        return MapToDetail(a);
    }
}

file static class ArticleMapping
{
    internal static ArticleDetailDto MapToDetail(Article a) =>
        new(a.Id, a.Title, a.Slug, a.Excerpt, a.Content, a.ThumbnailUrl, a.CategoryId, a.Category?.Name,
            a.AuthorId, a.Status, a.PublishedAt, a.ScheduledAt, a.ViewCount,
            a.MetaTitle, a.MetaDescription, a.CanonicalUrl, a.OgImage, a.CreatedAt);
}
