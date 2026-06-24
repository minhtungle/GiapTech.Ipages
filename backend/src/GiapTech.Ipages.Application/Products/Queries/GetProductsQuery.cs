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
    string? ShortDescription,
    string? Description,
    decimal Price,
    decimal? SalePrice,
    DateTime? SalePriceFrom,
    DateTime? SalePriceTo,
    decimal? CostPerItem,
    string? ThumbnailUrl,
    string? Images,
    string? VideoUrl,
    Guid? CategoryId,
    string? CategoryName,
    string? TagsJson,
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

public record ProductVariantDto(Guid Id, string Name, string? Sku, decimal Price, decimal? SalePrice, decimal? Weight, int StockQuantity, string? ImageUrl, string? AttributeValues, int SortOrder, bool IsActive);
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
        IQueryable<Product> query = _db.Products.AsNoTracking().Include(p => p.Category);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search) || p.Slug.Contains(request.Search) || (p.Sku != null && p.Sku.Contains(request.Search)));

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
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

        return ProductMapping.MapToDetail(p);
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

        return ProductMapping.MapToDetail(p);
    }
}

internal static class ProductMapping
{
    internal static ProductDetailDto MapToDetail(Product p) =>
        new(p.Id, p.Name, p.Slug, p.Sku, p.ShortDescription, p.Description,
            p.Price, p.SalePrice, p.SalePriceFrom, p.SalePriceTo, p.CostPerItem,
            p.ThumbnailUrl, p.Images, p.VideoUrl,
            p.CategoryId, p.Category?.Name, p.TagsJson,
            p.StockQuantity, p.TrackInventory, p.Status, p.SortOrder, p.SoldCount,
            p.MetaTitle, p.MetaDescription, p.CanonicalUrl,
            p.Variants.OrderBy(v => v.SortOrder).Select(v => new ProductVariantDto(v.Id, v.Name, v.Sku, v.Price, v.SalePrice, v.Weight, v.StockQuantity, v.ImageUrl, v.AttributeValues, v.SortOrder, v.IsActive)),
            p.Attributes.OrderBy(a => a.SortOrder).Select(a => new ProductAttributeDto(a.Id, a.Name, a.Values, a.SortOrder)),
            p.CreatedAt);
}
