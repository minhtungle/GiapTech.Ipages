using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Menus.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Menus.Commands;

// ── Create Menu ───────────────────────────────────────────────────────────────

public record CreateMenuCommand(string Name, string Location) : IRequest<MenuDto>;

public class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(50);
    }
}

public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, MenuDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateMenuCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant) { _db = db; _tenant = tenant; }

    public async Task<MenuDto> Handle(CreateMenuCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null) throw new ForbiddenException();

        var menu = new Menu { TenantId = _tenant.TenantId.Value, Name = request.Name, Location = request.Location };
        _db.Menus.Add(menu);
        await _db.SaveChangesAsync(ct);

        return new MenuDto(menu.Id, menu.Name, menu.Location, menu.IsActive, [], menu.CreatedAt);
    }
}

// ── Update Menu ───────────────────────────────────────────────────────────────

public record UpdateMenuCommand(Guid Id, string Name, string Location, bool IsActive) : IRequest<MenuDto>;

public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, MenuDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateMenuCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<MenuDto> Handle(UpdateMenuCommand request, CancellationToken ct)
    {
        var menu = await _db.Menus.Include(m => m.Items).FirstOrDefaultAsync(m => m.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Menu), request.Id);

        menu.Name = request.Name;
        menu.Location = request.Location;
        menu.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);

        var items = menu.Items.ToList();
        return new MenuDto(menu.Id, menu.Name, menu.Location, menu.IsActive, BuildTree(items, null), menu.CreatedAt);
    }

    private static IEnumerable<MenuItemDto> BuildTree(List<MenuItem> items, Guid? parentId) =>
        items.Where(i => i.ParentId == parentId).OrderBy(i => i.SortOrder)
             .Select(i => new MenuItemDto(i.Id, i.Label, i.Url, i.Target, i.Icon, i.ParentId, i.SortOrder, i.IsActive, BuildTree(items, i.Id)));
}

// ── Delete Menu ───────────────────────────────────────────────────────────────

public record DeleteMenuCommand(Guid Id) : IRequest;

public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteMenuCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteMenuCommand request, CancellationToken ct)
    {
        var menu = await _db.Menus.FirstOrDefaultAsync(m => m.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Menu), request.Id);

        _db.Menus.Remove(menu);
        await _db.SaveChangesAsync(ct);
    }
}

// ── Upsert MenuItem ───────────────────────────────────────────────────────────

public record UpsertMenuItemCommand(Guid? Id, Guid MenuId, string Label, string? Url, string? Target, string? Icon, Guid? ParentId, int SortOrder) : IRequest<MenuItemDto>;

public class UpsertMenuItemCommandHandler : IRequestHandler<UpsertMenuItemCommand, MenuItemDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public UpsertMenuItemCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant) { _db = db; _tenant = tenant; }

    public async Task<MenuItemDto> Handle(UpsertMenuItemCommand request, CancellationToken ct)
    {
        MenuItem item;

        if (request.Id.HasValue)
        {
            item = await _db.MenuItems.FirstOrDefaultAsync(i => i.Id == request.Id.Value, ct)
                ?? throw new NotFoundException(nameof(MenuItem), request.Id.Value);
        }
        else
        {
            if (_tenant.TenantId == null) throw new ForbiddenException();
            item = new MenuItem { TenantId = _tenant.TenantId.Value, MenuId = request.MenuId };
            _db.MenuItems.Add(item);
        }

        item.Label = request.Label;
        item.Url = request.Url;
        item.Target = request.Target;
        item.Icon = request.Icon;
        item.ParentId = request.ParentId;
        item.SortOrder = request.SortOrder;

        await _db.SaveChangesAsync(ct);
        return new MenuItemDto(item.Id, item.Label, item.Url, item.Target, item.Icon, item.ParentId, item.SortOrder, item.IsActive, []);
    }
}

// ── Delete MenuItem ───────────────────────────────────────────────────────────

public record DeleteMenuItemCommand(Guid Id) : IRequest;

public class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteMenuItemCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteMenuItemCommand request, CancellationToken ct)
    {
        var item = await _db.MenuItems.FirstOrDefaultAsync(i => i.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(MenuItem), request.Id);

        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync(ct);
    }
}
