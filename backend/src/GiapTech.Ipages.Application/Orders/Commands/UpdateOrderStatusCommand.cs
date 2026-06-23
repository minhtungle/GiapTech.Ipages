using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Orders.Commands;

public record UpdateOrderStatusCommand(Guid Id, OrderStatus Status, string? CancelReason = null) : IRequest;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Order), request.Id);

        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Status", "Không thể thay đổi trạng thái đơn hàng đã hoàn thành hoặc đã hủy.") });

        order.Status = request.Status;

        switch (request.Status)
        {
            case OrderStatus.Confirmed:
                order.ConfirmedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Shipping:
                order.ShippedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Completed:
                order.CompletedAt = DateTime.UtcNow;
                order.PaymentStatus = PaymentStatus.Completed;
                break;
            case OrderStatus.Cancelled:
                order.CancelledAt = DateTime.UtcNow;
                order.CancelReason = request.CancelReason;
                break;
        }

        await _db.SaveChangesAsync(ct);
    }
}

public record CancelOrderCommand(Guid Id, string? Reason = null) : IRequest;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IApplicationDbContext _db;

    public CancelOrderCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Order), request.Id);

        if (order.Status == OrderStatus.Shipping || order.Status == OrderStatus.Completed)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Status", "Không thể hủy đơn hàng đang giao hoặc đã hoàn thành.") });

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancelReason = request.Reason;

        await _db.SaveChangesAsync(ct);
    }
}
