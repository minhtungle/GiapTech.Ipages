using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Customers.Queries;

public record CustomerDto(
    Guid Id,
    string FullName,
    string? Email,
    string? Phone,
    string? AvatarUrl,
    DateTime? DateOfBirth,
    string? Gender,
    bool IsActive,
    int LoyaltyPoints,
    string? Notes,
    int OrderCount,
    DateTime CreatedAt);

public record CustomerDetailDto(
    Guid Id,
    string FullName,
    string? Email,
    string? Phone,
    string? AvatarUrl,
    DateTime? DateOfBirth,
    string? Gender,
    bool IsActive,
    int LoyaltyPoints,
    string? Notes,
    IEnumerable<CustomerAddressDto> Addresses,
    DateTime CreatedAt);

public record CustomerAddressDto(Guid Id, string FullName, string Phone, string Address, string? Ward, string? District, string? Province, bool IsDefault);

public record GetCustomersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<PaginatedList<CustomerDto>>;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PaginatedList<CustomerDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCustomersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        IQueryable<Customer> query = _db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.FullName.Contains(request.Search) || (c.Email != null && c.Email.Contains(request.Search)) || (c.Phone != null && c.Phone.Contains(request.Search)));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerDto(c.Id, c.FullName, c.Email, c.Phone, c.AvatarUrl, c.DateOfBirth, c.Gender, c.IsActive, c.LoyaltyPoints, c.Notes, c.Orders.Count, c.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<CustomerDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDetailDto>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetCustomerByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CustomerDetailDto> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var c = await _db.Customers.AsNoTracking()
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Customer), request.Id);

        return new CustomerDetailDto(c.Id, c.FullName, c.Email, c.Phone, c.AvatarUrl, c.DateOfBirth, c.Gender, c.IsActive, c.LoyaltyPoints, c.Notes,
            c.Addresses.Select(a => new CustomerAddressDto(a.Id, a.FullName, a.Phone, a.Address, a.Ward, a.District, a.Province, a.IsDefault)),
            c.CreatedAt);
    }
}
