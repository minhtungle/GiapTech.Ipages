using GiapTech.Ipages.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Menus.Queries;

public record MenuItemDto(Guid Id, string Label, string? Url, string? Target, string? Icon, Guid? ParentId, int SortOrder, bool IsActive, IEnumerable<MenuItemDto> Children);

public record MenuDto(Guid Id, string Name, string Location, bool IsActive, IEnumerable<MenuItemDto> Items, DateTime CreatedAt);

public record GetMenusQuery : IRequest<IEnumerable<MenuDto>>;

public class GetMenusQueryHandler : IRequestHandler<GetMenusQuery, IEnumerable<MenuDto>>
{
    private readonly IApplicationDbContext _db;

    public GetMenusQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<MenuDto>> Handle(GetMenusQuery request, CancellationToken ct)
    {
        var menus = await _db.Menus.AsNoTracking()
            .Include(m => m.Items)
            .OrderBy(m => m.Name)
            .ToListAsync(ct);

        return menus.Select(m => new MenuDto(m.Id, m.Name, m.Location, m.IsActive, BuildTree(m.Items.ToList(), null), m.CreatedAt));
    }

    private static IEnumerable<MenuItemDto> BuildTree(List<Domain.Entities.MenuItem> items, Guid? parentId) =>
        items.Where(i => i.ParentId == parentId)
             .OrderBy(i => i.SortOrder)
             .Select(i => new MenuItemDto(i.Id, i.Label, i.Url, i.Target, i.Icon, i.ParentId, i.SortOrder, i.IsActive, BuildTree(items, i.Id)));
}

public record GetMenuByLocationQuery(string Location) : IRequest<MenuDto?>;

public class GetMenuByLocationQueryHandler : IRequestHandler<GetMenuByLocationQuery, MenuDto?>
{
    private readonly IApplicationDbContext _db;

    public GetMenuByLocationQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<MenuDto?> Handle(GetMenuByLocationQuery request, CancellationToken ct)
    {
        var m = await _db.Menus.AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Location == request.Location && x.IsActive, ct);

        if (m == null) return null;

        var items = m.Items.ToList();
        return new MenuDto(m.Id, m.Name, m.Location, m.IsActive, BuildTree(items, null), m.CreatedAt);
    }

    private static IEnumerable<MenuItemDto> BuildTree(List<Domain.Entities.MenuItem> items, Guid? parentId) =>
        items.Where(i => i.ParentId == parentId).OrderBy(i => i.SortOrder)
             .Select(i => new MenuItemDto(i.Id, i.Label, i.Url, i.Target, i.Icon, i.ParentId, i.SortOrder, i.IsActive, BuildTree(items, i.Id)));
}
