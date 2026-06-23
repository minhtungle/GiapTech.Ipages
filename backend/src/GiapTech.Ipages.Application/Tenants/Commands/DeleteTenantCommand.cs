using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Tenants.Commands;

public record DeleteTenantCommand(Guid Id) : IRequest;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteTenantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteTenantCommand request, CancellationToken ct)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Tenant), request.Id);

        _db.Tenants.Remove(tenant);
        await _db.SaveChangesAsync(ct);
    }
}
