using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Tenants.Queries;

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string? Email,
    string? Phone,
    string? Address,
    string? Description,
    string? LogoUrl,
    TenantStatus Status,
    DateTime? ExpiresAt,
    DateTime CreatedAt);

public record GetTenantsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    TenantStatus? Status = null) : IRequest<PaginatedList<TenantDto>>;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, PaginatedList<TenantDto>>
{
    private readonly IApplicationDbContext _db;

    public GetTenantsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken ct)
    {
        var query = _db.Tenants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.Name.Contains(request.Search) || t.Slug.Contains(request.Search) || (t.Email != null && t.Email.Contains(request.Search)));

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TenantDto(t.Id, t.Name, t.Slug, t.Email, t.Phone, t.Address, t.Description, t.LogoUrl, t.Status, t.ExpiresAt, t.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<TenantDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly IApplicationDbContext _db;

    public GetTenantByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<TenantDto> Handle(GetTenantByIdQuery request, CancellationToken ct)
    {
        var t = await _db.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Tenant), request.Id);

        return new TenantDto(t.Id, t.Name, t.Slug, t.Email, t.Phone, t.Address, t.Description, t.LogoUrl, t.Status, t.ExpiresAt, t.CreatedAt);
    }
}
