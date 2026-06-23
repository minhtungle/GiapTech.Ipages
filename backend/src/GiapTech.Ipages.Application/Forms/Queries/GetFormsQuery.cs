using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Common.Models;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Forms.Queries;

public record FormDto(Guid Id, string Name, string? Description, FormType Type, string Fields, string? SuccessMessage, string? NotifyEmails, bool IsActive, int EntryCount, DateTime CreatedAt);

public record FormEntryDto(Guid Id, Guid FormId, string Data, string? IpAddress, bool IsRead, DateTime CreatedAt);

public record GetFormsQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedList<FormDto>>;

public class GetFormsQueryHandler : IRequestHandler<GetFormsQuery, PaginatedList<FormDto>>
{
    private readonly IApplicationDbContext _db;

    public GetFormsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<FormDto>> Handle(GetFormsQuery request, CancellationToken ct)
    {
        var total = await _db.Forms.CountAsync(ct);
        var items = await _db.Forms.AsNoTracking()
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FormDto(f.Id, f.Name, f.Description, f.Type, f.Fields, f.SuccessMessage, f.NotifyEmails, f.IsActive, f.Entries.Count, f.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<FormDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetFormEntriesQuery(Guid FormId, int Page = 1, int PageSize = 20, bool? IsRead = null) : IRequest<PaginatedList<FormEntryDto>>;

public class GetFormEntriesQueryHandler : IRequestHandler<GetFormEntriesQuery, PaginatedList<FormEntryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetFormEntriesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<FormEntryDto>> Handle(GetFormEntriesQuery request, CancellationToken ct)
    {
        IQueryable<FormEntry> query = _db.FormEntries.AsNoTracking().Where(e => e.FormId == request.FormId);

        if (request.IsRead.HasValue)
            query = query.Where(e => e.IsRead == request.IsRead.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new FormEntryDto(e.Id, e.FormId, e.Data, e.IpAddress, e.IsRead, e.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedList<FormEntryDto>(items, total, request.Page, request.PageSize);
    }
}
