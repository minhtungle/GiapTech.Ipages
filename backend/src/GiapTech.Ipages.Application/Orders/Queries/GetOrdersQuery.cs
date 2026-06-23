using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Orders.Queries;

public record OrderListDto(
    Guid Id,
    string OrderCode,
    string CustomerName,
    string? CustomerEmail,
    string CustomerPhone,
    decimal Total,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    DateTime CreatedAt);

public record OrderDetailDto(
    Guid Id,
    string OrderCode,
    Guid? CustomerId,
    string CustomerName,
    string? CustomerEmail,
    string CustomerPhone,
    string ShippingAddress,
    string? ShippingWard,
    string? ShippingDistrict,
    string? ShippingProvince,
    decimal SubTotal,
    decimal ShippingFee,
    decimal Discount,
    decimal Total,
    OrderStatus Status,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    string? Notes,
    string? CouponCode,
    string? CancelReason,
    DateTime? ConfirmedAt,
    DateTime? ShippedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    IEnumerable<OrderItemDto> Items,
    DateTime CreatedAt);

public record OrderItemDto(Guid Id, Guid ProductId, Guid? VariantId, string ProductName, string? VariantName, string? Sku, decimal UnitPrice, int Quantity, decimal Total);

public record GetOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    OrderStatus? Status = null) : IRequest<PaginatedList<OrderListDto>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PaginatedList<OrderListDto>>
{
    private readonly IApplicationDbContext _db;

    public GetOrdersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<OrderListDto>> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        IQueryable<Order> query = _db.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(o => o.OrderCode.Contains(request.Search) || o.CustomerName.Contains(request.Search) || o.CustomerPhone.Contains(request.Search));

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderListDto(o.Id, o.OrderCode, o.CustomerName, o.CustomerEmail, o.CustomerPhone, o.Total, o.Status, o.PaymentMethod, o.PaymentStatus, o.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<OrderListDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDetailDto>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetOrderByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<OrderDetailDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var o = await _db.Orders.AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Order), request.Id);

        return new OrderDetailDto(o.Id, o.OrderCode, o.CustomerId, o.CustomerName, o.CustomerEmail, o.CustomerPhone,
            o.ShippingAddress, o.ShippingWard, o.ShippingDistrict, o.ShippingProvince,
            o.SubTotal, o.ShippingFee, o.Discount, o.Total, o.Status, o.PaymentMethod, o.PaymentStatus,
            o.Notes, o.CouponCode, o.CancelReason, o.ConfirmedAt, o.ShippedAt, o.CompletedAt, o.CancelledAt,
            o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.VariantId, i.ProductName, i.VariantName, i.Sku, i.UnitPrice, i.Quantity, i.Total)),
            o.CreatedAt);
    }
}
