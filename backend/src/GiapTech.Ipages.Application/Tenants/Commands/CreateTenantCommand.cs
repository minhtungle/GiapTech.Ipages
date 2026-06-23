using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Tenants.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Tenants.Commands;

public record CreateTenantCommand(
    string Name,
    string Slug,
    string? Email,
    string? Phone,
    string? Address,
    string? Description,
    TenantStatus Status,
    DateTime? ExpiresAt) : IRequest<TenantDto>;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-z0-9-]+$").WithMessage("Slug chỉ chứa chữ thường, số và dấu gạch ngang.");
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
    }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _db;

    public CreateTenantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        var slugExists = await _db.Tenants.AnyAsync(t => t.Slug == request.Slug, ct);
        if (slugExists)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Slug", "Slug đã tồn tại.") });

        var tenant = new Tenant
        {
            Name = request.Name,
            Slug = request.Slug,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Description = request.Description,
            Status = request.Status,
            ExpiresAt = request.ExpiresAt
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync(ct);

        return new TenantDto(tenant.Id, tenant.Name, tenant.Slug, tenant.Email, tenant.Phone, tenant.Address, tenant.Description, tenant.LogoUrl, tenant.Status, tenant.ExpiresAt, tenant.CreatedAt);
    }
}
