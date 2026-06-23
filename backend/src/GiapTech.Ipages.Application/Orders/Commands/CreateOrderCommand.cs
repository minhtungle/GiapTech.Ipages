using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Orders.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Orders.Commands;

public record CreateOrderItemInput(Guid ProductId, Guid? VariantId, int Quantity);

public record CreateOrderCommand(
    Guid? CustomerId,
    string CustomerName,
    string? CustomerEmail,
    string CustomerPhone,
    string ShippingAddress,
    string? ShippingWard,
    string? ShippingDistrict,
    string? ShippingProvince,
    decimal ShippingFee,
    PaymentMethod PaymentMethod,
    string? Notes,
    string? CouponCode,
    IEnumerable<CreateOrderItemInput> Items) : IRequest<OrderDetailDto>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ShippingAddress).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Đơn hàng phải có ít nhất 1 sản phẩm.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateOrderCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<OrderDetailDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null)
            throw new ForbiddenException();

        var tenantId = _tenant.TenantId.Value;
        var orderCode = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

        var orderItems = new List<OrderItem>();
        decimal subTotal = 0;

        foreach (var item in request.Items)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, ct)
                ?? throw new NotFoundException(nameof(Product), item.ProductId);

            var unitPrice = product.SalePrice ?? product.Price;
            string? variantName = null;

            if (item.VariantId.HasValue)
            {
                var variant = await _db.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.VariantId.Value, ct);
                if (variant != null)
                {
                    unitPrice = variant.SalePrice ?? variant.Price;
                    variantName = variant.Name;
                }
            }

            var lineTotal = unitPrice * item.Quantity;
            subTotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                TenantId = tenantId,
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                ProductName = product.Name,
                VariantName = variantName,
                Sku = product.Sku,
                ImageUrl = product.ThumbnailUrl,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                Total = lineTotal
            });
        }

        var order = new Order
        {
            TenantId = tenantId,
            OrderCode = orderCode,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ShippingAddress = request.ShippingAddress,
            ShippingWard = request.ShippingWard,
            ShippingDistrict = request.ShippingDistrict,
            ShippingProvince = request.ShippingProvince,
            SubTotal = subTotal,
            ShippingFee = request.ShippingFee,
            Discount = 0,
            Total = subTotal + request.ShippingFee,
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            CouponCode = request.CouponCode,
            Items = orderItems
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return new OrderDetailDto(order.Id, order.OrderCode, order.CustomerId, order.CustomerName, order.CustomerEmail, order.CustomerPhone,
            order.ShippingAddress, order.ShippingWard, order.ShippingDistrict, order.ShippingProvince,
            order.SubTotal, order.ShippingFee, order.Discount, order.Total, order.Status, order.PaymentMethod, order.PaymentStatus,
            order.Notes, order.CouponCode, order.CancelReason, order.ConfirmedAt, order.ShippedAt, order.CompletedAt, order.CancelledAt,
            order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.VariantId, i.ProductName, i.VariantName, i.Sku, i.UnitPrice, i.Quantity, i.Total)),
            order.CreatedAt);
    }
}
