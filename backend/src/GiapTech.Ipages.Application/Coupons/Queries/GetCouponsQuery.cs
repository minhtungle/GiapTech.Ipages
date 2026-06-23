using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Coupons.Queries;

public record CouponDto(
    Guid Id,
    string Code,
    string? Name,
    string? Description,
    CouponType Type,
    decimal Value,
    decimal? MinOrderAmount,
    decimal? MaxDiscount,
    int? UsageLimit,
    int UsedCount,
    DateTime? StartsAt,
    DateTime? ExpiresAt,
    bool IsActive);

public record GetCouponsQuery(int Page = 1, int PageSize = 20, string? Search = null, bool? IsActive = null) : IRequest<PaginatedList<CouponDto>>;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, PaginatedList<CouponDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCouponsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<CouponDto>> Handle(GetCouponsQuery request, CancellationToken ct)
    {
        IQueryable<Coupon> query = _db.Coupons.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.Code.Contains(request.Search) || (c.Name != null && c.Name.Contains(request.Search)));

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CouponDto(c.Id, c.Code, c.Name, c.Description, c.Type, c.Value, c.MinOrderAmount, c.MaxDiscount, c.UsageLimit, c.UsedCount, c.StartsAt, c.ExpiresAt, c.IsActive))
            .ToListAsync(ct);

        return new PaginatedList<CouponDto>(items, total, request.Page, request.PageSize);
    }
}
