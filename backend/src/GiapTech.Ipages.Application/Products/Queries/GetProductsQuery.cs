using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Products.Queries;

public record ProductListDto(
    Guid Id,
    string Name,
    string Slug,
    string? Sku,
    string? ShortDescription,
    decimal Price,
    decimal? SalePrice,
    string? ThumbnailUrl,
    Guid? CategoryId,
    string? CategoryName,
    int StockQuantity,
    ProductStatus Status,
    int SoldCount,
    DateTime CreatedAt);

public record ProductDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string? Sku,
    string? Description,
    string? ShortDescription,
    decimal Price,
    decimal? SalePrice,
    string? ThumbnailUrl,
    string? Images,
    Guid? CategoryId,
    string? CategoryName,
    int StockQuantity,
    bool TrackInventory,
    ProductStatus Status,
    int SortOrder,
    int SoldCount,
    string? MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    IEnumerable<ProductVariantDto> Variants,
    IEnumerable<ProductAttributeDto> Attributes,
    DateTime CreatedAt);

public record ProductVariantDto(Guid Id, string Name, string? Sku, decimal Price, decimal? SalePrice, int StockQuantity, string? ImageUrl, bool IsActive);
public record ProductAttributeDto(Guid Id, string Name, string Values, int SortOrder);

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? CategoryId = null,
    ProductStatus? Status = null) : IRequest<PaginatedList<ProductListDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductListDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProductsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<ProductListDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var query = _db.Products.AsNoTracking().Include(p => p.Category);

        IQueryable<Product> filtered = query;

        if (!string.IsNullOrWhiteSpace(request.Search))
            filtered = filtered.Where(p => p.Name.Contains(request.Search) || p.Slug.Contains(request.Search) || (p.Sku != null && p.Sku.Contains(request.Search)));

        if (request.CategoryId.HasValue)
            filtered = filtered.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.Status.HasValue)
            filtered = filtered.Where(p => p.Status == request.Status.Value);

        var total = await filtered.CountAsync(ct);
        var items = await filtered
            .OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListDto(p.Id, p.Name, p.Slug, p.Sku, p.ShortDescription, p.Price, p.SalePrice, p.ThumbnailUrl, p.CategoryId, p.Category != null ? p.Category.Name : null, p.StockQuantity, p.Status, p.SoldCount, p.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<ProductListDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDetailDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetProductByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var p = await _db.Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Variants)
            .Include(x => x.Attributes)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Product), request.Id);

        return MapToDetail(p);
    }
}

public record GetProductBySlugQuery(string Slug) : IRequest<ProductDetailDto>, ICacheable
{
    public string CacheKey => $"product:slug:{Slug}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(15);
}

public class GetProductBySlugQueryHandler : IRequestHandler<GetProductBySlugQuery, ProductDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetProductBySlugQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductDetailDto> Handle(GetProductBySlugQuery request, CancellationToken ct)
    {
        var p = await _db.Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Variants)
            .Include(x => x.Attributes)
            .FirstOrDefaultAsync(x => x.Slug == request.Slug && x.Status == ProductStatus.Active, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Product), request.Slug);

        return MapToDetail(p);
    }
}

file static class ProductMapping
{
    internal static ProductDetailDto MapToDetail(Product p) =>
        new(p.Id, p.Name, p.Slug, p.Sku, p.Description, p.ShortDescription, p.Price, p.SalePrice,
            p.ThumbnailUrl, p.Images, p.CategoryId, p.Category?.Name, p.StockQuantity, p.TrackInventory,
            p.Status, p.SortOrder, p.SoldCount, p.MetaTitle, p.MetaDescription, p.CanonicalUrl,
            p.Variants.Select(v => new ProductVariantDto(v.Id, v.Name, v.Sku, v.Price, v.SalePrice, v.StockQuantity, v.ImageUrl, v.IsActive)),
            p.Attributes.Select(a => new ProductAttributeDto(a.Id, a.Name, a.Values, a.SortOrder)),
            p.CreatedAt);
}
