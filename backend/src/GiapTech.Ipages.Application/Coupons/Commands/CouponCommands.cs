using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using ValidationException = GiapTech.Ipages.Application.Common.Exceptions.ValidationException;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Coupons.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Coupons.Commands;

// ── Create ────────────────────────────────────────────────────────────────────

public record CreateCouponCommand(
    string Code,
    string? Name,
    string? Description,
    CouponType Type,
    decimal Value,
    decimal? MinOrderAmount,
    decimal? MaxDiscount,
    int? UsageLimit,
    DateTime? StartsAt,
    DateTime? ExpiresAt) : IRequest<CouponDto>;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).Matches("^[A-Z0-9_-]+$").WithMessage("Code chỉ chứa chữ hoa, số, gạch ngang và gạch dưới.");
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.Value).LessThanOrEqualTo(100).When(x => x.Type == CouponType.Percentage).WithMessage("Phần trăm giảm không vượt quá 100.");
    }
}

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CouponDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateCouponCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant) { _db = db; _tenant = tenant; }

    public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null) throw new ForbiddenException();

        var exists = await _db.Coupons.AnyAsync(c => c.Code == request.Code, ct);
        if (exists)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Code", "Mã giảm giá đã tồn tại.") });

        var coupon = new Coupon
        {
            TenantId = _tenant.TenantId.Value,
            Code = request.Code.ToUpperInvariant(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Value = request.Value,
            MinOrderAmount = request.MinOrderAmount,
            MaxDiscount = request.MaxDiscount,
            UsageLimit = request.UsageLimit,
            StartsAt = request.StartsAt,
            ExpiresAt = request.ExpiresAt
        };

        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync(ct);

        return new CouponDto(coupon.Id, coupon.Code, coupon.Name, coupon.Description, coupon.Type, coupon.Value, coupon.MinOrderAmount, coupon.MaxDiscount, coupon.UsageLimit, coupon.UsedCount, coupon.StartsAt, coupon.ExpiresAt, coupon.IsActive);
    }
}

// ── Update ────────────────────────────────────────────────────────────────────

public record UpdateCouponCommand(Guid Id, string? Name, string? Description, decimal Value, decimal? MinOrderAmount, decimal? MaxDiscount, int? UsageLimit, DateTime? StartsAt, DateTime? ExpiresAt, bool IsActive) : IRequest<CouponDto>;

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, CouponDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateCouponCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<CouponDto> Handle(UpdateCouponCommand request, CancellationToken ct)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Coupon), request.Id);

        coupon.Name = request.Name;
        coupon.Description = request.Description;
        coupon.Value = request.Value;
        coupon.MinOrderAmount = request.MinOrderAmount;
        coupon.MaxDiscount = request.MaxDiscount;
        coupon.UsageLimit = request.UsageLimit;
        coupon.StartsAt = request.StartsAt;
        coupon.ExpiresAt = request.ExpiresAt;
        coupon.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);

        return new CouponDto(coupon.Id, coupon.Code, coupon.Name, coupon.Description, coupon.Type, coupon.Value, coupon.MinOrderAmount, coupon.MaxDiscount, coupon.UsageLimit, coupon.UsedCount, coupon.StartsAt, coupon.ExpiresAt, coupon.IsActive);
    }
}

// ── Delete ────────────────────────────────────────────────────────────────────

public record DeleteCouponCommand(Guid Id) : IRequest;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteCouponCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteCouponCommand request, CancellationToken ct)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Coupon), request.Id);

        _db.Coupons.Remove(coupon);
        await _db.SaveChangesAsync(ct);
    }
}

// ── Validate (returns discount amount, does NOT increment usedCount) ──────────

public record ValidateCouponResult(bool IsValid, string? Error, decimal DiscountAmount, CouponDto? Coupon);

public record ValidateCouponCommand(string Code, decimal OrderAmount) : IRequest<ValidateCouponResult>;

public class ValidateCouponCommandHandler : IRequestHandler<ValidateCouponCommand, ValidateCouponResult>
{
    private readonly IApplicationDbContext _db;

    public ValidateCouponCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ValidateCouponResult> Handle(ValidateCouponCommand request, CancellationToken ct)
    {
        var coupon = await _db.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code == request.Code, ct);
        if (coupon == null) return new ValidateCouponResult(false, "Mã giảm giá không tồn tại.", 0, null);
        if (!coupon.IsActive) return new ValidateCouponResult(false, "Mã giảm giá đã bị vô hiệu hóa.", 0, null);

        var now = DateTime.UtcNow;
        if (coupon.StartsAt.HasValue && now < coupon.StartsAt) return new ValidateCouponResult(false, "Mã giảm giá chưa có hiệu lực.", 0, null);
        if (coupon.ExpiresAt.HasValue && now > coupon.ExpiresAt) return new ValidateCouponResult(false, "Mã giảm giá đã hết hạn.", 0, null);
        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit) return new ValidateCouponResult(false, "Mã giảm giá đã hết lượt sử dụng.", 0, null);
        if (coupon.MinOrderAmount.HasValue && request.OrderAmount < coupon.MinOrderAmount) return new ValidateCouponResult(false, $"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0}đ.", 0, null);

        var discount = coupon.Type == CouponType.Percentage
            ? request.OrderAmount * coupon.Value / 100
            : coupon.Value;

        if (coupon.MaxDiscount.HasValue && discount > coupon.MaxDiscount.Value)
            discount = coupon.MaxDiscount.Value;

        var dto = new CouponDto(coupon.Id, coupon.Code, coupon.Name, coupon.Description, coupon.Type, coupon.Value, coupon.MinOrderAmount, coupon.MaxDiscount, coupon.UsageLimit, coupon.UsedCount, coupon.StartsAt, coupon.ExpiresAt, coupon.IsActive);
        return new ValidateCouponResult(true, null, discount, dto);
    }
}
