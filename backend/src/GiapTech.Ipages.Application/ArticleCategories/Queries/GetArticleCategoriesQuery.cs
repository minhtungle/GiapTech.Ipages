using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.ArticleCategories.Queries;

public record ArticleCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentId,
    string? ParentName,
    int SortOrder,
    bool IsActive,
    int ArticleCount);

public record GetArticleCategoriesQuery(bool ActiveOnly = false) : IRequest<IEnumerable<ArticleCategoryDto>>;

public class GetArticleCategoriesQueryHandler : IRequestHandler<GetArticleCategoriesQuery, IEnumerable<ArticleCategoryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetArticleCategoriesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<ArticleCategoryDto>> Handle(GetArticleCategoriesQuery request, CancellationToken ct)
    {
        IQueryable<ArticleCategory> query = _db.ArticleCategories.AsNoTracking().Include(c => c.Parent);

        if (request.ActiveOnly)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new ArticleCategoryDto(c.Id, c.Name, c.Slug, c.Description, c.ParentId, c.Parent != null ? c.Parent.Name : null, c.SortOrder, c.IsActive, c.Articles.Count))
            .ToListAsync(ct);
    }
}
