using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.ApiKeys.Queries;

public record ApiKeyDto(
    Guid Id,
    string Name,
    string Key,
    string? AllowedOrigins,
    string? Permissions,
    bool IsActive,
    DateTime? ExpiresAt,
    DateTime? LastUsedAt,
    long RequestCount,
    DateTime CreatedAt);

public record GetApiKeysQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedList<ApiKeyDto>>;

public class GetApiKeysQueryHandler : IRequestHandler<GetApiKeysQuery, PaginatedList<ApiKeyDto>>
{
    private readonly IApplicationDbContext _db;

    public GetApiKeysQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<ApiKeyDto>> Handle(GetApiKeysQuery request, CancellationToken ct)
    {
        var total = await _db.ApiKeys.CountAsync(ct);
        var items = await _db.ApiKeys.AsNoTracking()
            .OrderByDescending(k => k.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(k => new ApiKeyDto(k.Id, k.Name, k.Key, k.AllowedOrigins, k.Permissions, k.IsActive, k.ExpiresAt, k.LastUsedAt, k.RequestCount, k.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<ApiKeyDto>(items, total, request.Page, request.PageSize);
    }
}
