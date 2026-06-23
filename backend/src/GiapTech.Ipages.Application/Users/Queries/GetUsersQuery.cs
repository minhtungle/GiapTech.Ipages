using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Users.Queries;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string? FullName,
    string? PhoneNumber,
    string? AvatarUrl,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<PaginatedList<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetUsersQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var query = _db.Users.AsNoTracking().Where(u => u.TenantId == _tenant.TenantId && !u.IsHostAdmin);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(u => u.Username.Contains(request.Search) || u.Email.Contains(request.Search) || (u.FullName != null && u.FullName.Contains(request.Search)));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto(u.Id, u.Username, u.Email, u.FullName, u.PhoneNumber, u.AvatarUrl, u.IsActive, u.LastLoginAt, u.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<UserDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetUserByIdQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var u = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == _tenant.TenantId, ct)
            ?? throw new Common.Exceptions.NotFoundException("User", request.Id);

        return new UserDto(u.Id, u.Username, u.Email, u.FullName, u.PhoneNumber, u.AvatarUrl, u.IsActive, u.LastLoginAt, u.CreatedAt);
    }
}
