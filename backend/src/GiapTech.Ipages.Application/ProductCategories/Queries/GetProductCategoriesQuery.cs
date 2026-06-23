using GiapTech.Ipages.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.ProductCategories.Queries;

public record ProductCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    Guid? ParentId,
    string? ParentName,
    int SortOrder,
    bool IsActive,
    int ProductCount);

public record GetProductCategoriesQuery(bool ActiveOnly = false) : IRequest<IEnumerable<ProductCategoryDto>>;

public class GetProductCategoriesQueryHandler : IRequestHandler<GetProductCategoriesQuery, IEnumerable<ProductCategoryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetProductCategoriesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<ProductCategoryDto>> Handle(GetProductCategoriesQuery request, CancellationToken ct)
    {
        var query = _db.ProductCategories.AsNoTracking().Include(c => c.Parent);

        if (request.ActiveOnly)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new ProductCategoryDto(c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.ParentId, c.Parent != null ? c.Parent.Name : null, c.SortOrder, c.IsActive, c.Products.Count))
            .ToListAsync(ct);
    }
}
