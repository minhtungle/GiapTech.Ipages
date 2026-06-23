using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Tenants.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Tenants.Commands;

public record UpdateTenantCommand(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string? Address,
    string? Description,
    TenantStatus Status,
    DateTime? ExpiresAt) : IRequest<TenantDto>;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
    }
}

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateTenantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken ct)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Tenant), request.Id);

        tenant.Name = request.Name;
        tenant.Email = request.Email;
        tenant.Phone = request.Phone;
        tenant.Address = request.Address;
        tenant.Description = request.Description;
        tenant.Status = request.Status;
        tenant.ExpiresAt = request.ExpiresAt;

        await _db.SaveChangesAsync(ct);

        return new TenantDto(tenant.Id, tenant.Name, tenant.Slug, tenant.Email, tenant.Phone, tenant.Address, tenant.Description, tenant.LogoUrl, tenant.Status, tenant.ExpiresAt, tenant.CreatedAt);
    }
}
