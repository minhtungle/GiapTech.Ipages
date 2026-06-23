using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Dashboard.Queries;

public record HostDashboardDto(
    int TotalTenants,
    int ActiveTenants,
    int InactiveTenants,
    int NewTenantsThisMonth);

public record TenantDashboardDto(
    int TotalProducts,
    int ActiveProducts,
    int TotalOrders,
    int PendingOrders,
    int TodayOrders,
    decimal TodayRevenue,
    decimal ThisMonthRevenue,
    int TotalCustomers,
    int TotalArticles);

public record GetHostDashboardQuery : IRequest<HostDashboardDto>;

public class GetHostDashboardQueryHandler : IRequestHandler<GetHostDashboardQuery, HostDashboardDto>
{
    private readonly IApplicationDbContext _db;

    public GetHostDashboardQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<HostDashboardDto> Handle(GetHostDashboardQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var total = await _db.Tenants.CountAsync(ct);
        var active = await _db.Tenants.CountAsync(t => t.Status == TenantStatus.Active, ct);
        var inactive = await _db.Tenants.CountAsync(t => t.Status != TenantStatus.Active, ct);
        var newThisMonth = await _db.Tenants.CountAsync(t => t.CreatedAt >= firstOfMonth, ct);

        return new HostDashboardDto(total, active, inactive, newThisMonth);
    }
}

public record GetTenantDashboardQuery : IRequest<TenantDashboardDto>;

public class GetTenantDashboardQueryHandler : IRequestHandler<GetTenantDashboardQuery, TenantDashboardDto>
{
    private readonly IApplicationDbContext _db;

    public GetTenantDashboardQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<TenantDashboardDto> Handle(GetTenantDashboardQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalProducts = await _db.Products.CountAsync(ct);
        var activeProducts = await _db.Products.CountAsync(p => p.Status == ProductStatus.Active, ct);
        var totalOrders = await _db.Orders.CountAsync(ct);
        var pendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending, ct);
        var todayOrders = await _db.Orders.CountAsync(o => o.CreatedAt >= todayStart, ct);
        var todayRevenue = await _db.Orders
            .Where(o => o.CreatedAt >= todayStart && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.Total, ct);
        var monthRevenue = await _db.Orders
            .Where(o => o.CreatedAt >= firstOfMonth && o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.Total, ct);
        var totalCustomers = await _db.Customers.CountAsync(ct);
        var totalArticles = await _db.Articles.CountAsync(ct);

        return new TenantDashboardDto(totalProducts, activeProducts, totalOrders, pendingOrders, todayOrders, todayRevenue, monthRevenue, totalCustomers, totalArticles);
    }
}
