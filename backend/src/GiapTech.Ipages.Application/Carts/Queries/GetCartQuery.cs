using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Carts.Queries;

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    Guid? VariantId,
    string ProductName,
    string? VariantName,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal);

public record CartDto(
    Guid Id,
    Guid? CustomerId,
    string? SessionId,
    string? CouponCode,
    decimal Discount,
    decimal SubTotal,
    decimal Total,
    IEnumerable<CartItemDto> Items);

public record GetCartQuery(string? SessionId, Guid? CustomerId) : IRequest<CartDto?>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto?>
{
    private readonly IApplicationDbContext _db;

    public GetCartQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CartDto?> Handle(GetCartQuery request, CancellationToken ct)
    {
        Cart? cart = null;

        if (request.CustomerId.HasValue)
            cart = await _db.Carts.AsNoTracking().Include(c => c.Items).FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, ct);
        else if (!string.IsNullOrWhiteSpace(request.SessionId))
            cart = await _db.Carts.AsNoTracking().Include(c => c.Items).FirstOrDefaultAsync(c => c.SessionId == request.SessionId, ct);

        if (cart == null) return null;

        var subTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        return new CartDto(cart.Id, cart.CustomerId, cart.SessionId, cart.CouponCode, cart.Discount, subTotal, subTotal - cart.Discount,
            cart.Items.Select(i => new CartItemDto(i.Id, i.ProductId, i.VariantId, i.ProductName, i.VariantName, i.ImageUrl, i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity)));
    }
}
