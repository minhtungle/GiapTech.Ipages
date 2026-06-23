using FluentValidation;
using GiapTech.Ipages.Application.Carts.Queries;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Carts.Commands;

// ── Add to Cart ──────────────────────────────────────────────────────────────

public record AddToCartCommand(string? SessionId, Guid? CustomerId, Guid ProductId, Guid? VariantId, int Quantity) : IRequest<CartDto>;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x).Must(x => x.SessionId != null || x.CustomerId != null).WithMessage("SessionId hoặc CustomerId là bắt buộc.");
    }
}

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public AddToCartCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant) { _db = db; _tenant = tenant; }

    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null) throw new ForbiddenException();

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        decimal unitPrice = product.SalePrice ?? product.Price;
        string? variantName = null;

        if (request.VariantId.HasValue)
        {
            var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == request.VariantId.Value, ct);
            if (variant != null) { unitPrice = variant.SalePrice ?? variant.Price; variantName = variant.Name; }
        }

        Cart? cart = request.CustomerId.HasValue
            ? await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, ct)
            : await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.SessionId == request.SessionId, ct);

        if (cart == null)
        {
            cart = new Cart { TenantId = _tenant.TenantId.Value, CustomerId = request.CustomerId, SessionId = request.SessionId, ExpiresAt = DateTime.UtcNow.AddDays(30) };
            _db.Carts.Add(cart);
        }

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId && i.VariantId == request.VariantId);
        if (existing != null)
            existing.Quantity += request.Quantity;
        else
            cart.Items.Add(new CartItem { TenantId = _tenant.TenantId.Value, CartId = cart.Id, ProductId = request.ProductId, VariantId = request.VariantId, ProductName = product.Name, VariantName = variantName, ImageUrl = product.ThumbnailUrl, UnitPrice = unitPrice, Quantity = request.Quantity });

        await _db.SaveChangesAsync(ct);

        var subTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        return new CartDto(cart.Id, cart.CustomerId, cart.SessionId, cart.CouponCode, cart.Discount, subTotal, subTotal - cart.Discount,
            cart.Items.Select(i => new CartItemDto(i.Id, i.ProductId, i.VariantId, i.ProductName, i.VariantName, i.ImageUrl, i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity)));
    }
}

// ── Update Item Quantity ──────────────────────────────────────────────────────

public record UpdateCartItemCommand(Guid CartItemId, int Quantity) : IRequest;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateCartItemCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateCartItemCommand request, CancellationToken ct)
    {
        var item = await _db.CartItems.FirstOrDefaultAsync(i => i.Id == request.CartItemId, ct)
            ?? throw new NotFoundException(nameof(CartItem), request.CartItemId);

        if (request.Quantity <= 0)
            _db.CartItems.Remove(item);
        else
            item.Quantity = request.Quantity;

        await _db.SaveChangesAsync(ct);
    }
}

// ── Remove Item ───────────────────────────────────────────────────────────────

public record RemoveCartItemCommand(Guid CartItemId) : IRequest;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveCartItemCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(RemoveCartItemCommand request, CancellationToken ct)
    {
        var item = await _db.CartItems.FirstOrDefaultAsync(i => i.Id == request.CartItemId, ct)
            ?? throw new NotFoundException(nameof(CartItem), request.CartItemId);

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync(ct);
    }
}

// ── Clear Cart ────────────────────────────────────────────────────────────────

public record ClearCartCommand(Guid CartId) : IRequest;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand>
{
    private readonly IApplicationDbContext _db;

    public ClearCartCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(ClearCartCommand request, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == request.CartId, ct)
            ?? throw new NotFoundException(nameof(Cart), request.CartId);

        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync(ct);
    }
}
