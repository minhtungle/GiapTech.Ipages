using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Tenants.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = GiapTech.Ipages.Application.Common.Exceptions.ValidationException;

namespace GiapTech.Ipages.Application.Tenants.Commands;

public record UpdateTenantCommand(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string? Address,
    string? Description,
    TenantStatus Status,
    DateTime? ExpiresAt,
    string? AdminUsername,
    string? AdminPassword) : IRequest<TenantDto>;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.AdminPassword).MinimumLength(6)
            .When(x => !string.IsNullOrWhiteSpace(x.AdminPassword))
            .WithMessage("Mật khẩu admin phải có ít nhất 6 ký tự.");
    }
}

public class UpdateTenantCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher)
    : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken ct)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Tenant), request.Id);

        tenant.Name = request.Name;
        tenant.Email = request.Email;
        tenant.Phone = request.Phone;
        tenant.Address = request.Address;
        tenant.Description = request.Description;
        tenant.Status = request.Status;
        tenant.ExpiresAt = request.ExpiresAt;

        if (!string.IsNullOrWhiteSpace(request.AdminPassword))
        {
            var targetUsername = request.AdminUsername;
            var adminUser = string.IsNullOrWhiteSpace(targetUsername)
                ? await db.Users.FirstOrDefaultAsync(u => u.TenantId == request.Id && u.IsActive, ct)
                : await db.Users.FirstOrDefaultAsync(u => u.TenantId == request.Id && u.Username == targetUsername, ct);

            if (adminUser != null)
                adminUser.PasswordHash = passwordHasher.Hash(request.AdminPassword);
        }

        await db.SaveChangesAsync(ct);

        return new TenantDto(tenant.Id, tenant.Name, tenant.Slug, tenant.Email, tenant.Phone, tenant.Address, tenant.Description, tenant.LogoUrl, tenant.Status, tenant.ExpiresAt, tenant.CreatedAt);
    }
}
