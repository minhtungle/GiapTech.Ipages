using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using ValidationException = GiapTech.Ipages.Application.Common.Exceptions.ValidationException;
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
    DateTime? ExpiresAt,
    string? AdminUsername,
    string? AdminPassword) : IRequest<TenantDto>;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-z0-9-]+$").WithMessage("Slug chỉ chứa chữ thường, số và dấu gạch ngang.");
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(6)
            .When(x => !string.IsNullOrWhiteSpace(x.AdminUsername))
            .WithMessage("Mật khẩu admin phải có ít nhất 6 ký tự.");
        RuleFor(x => x.AdminUsername).NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.AdminPassword))
            .WithMessage("Tên đăng nhập admin không được bỏ trống.");
    }
}

public class CreateTenantCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher)
    : IRequestHandler<CreateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        var slugExists = await db.Tenants.AnyAsync(t => t.Slug == request.Slug, ct);
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
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(request.AdminUsername) && !string.IsNullOrWhiteSpace(request.AdminPassword))
        {
            var allPermissions = await db.Permissions.ToListAsync(ct);
            var adminRole = new Role
            {
                TenantId = tenant.Id,
                Name = "Admin",
                Description = "Full access",
                IsSystem = true
            };
            db.Roles.Add(adminRole);
            await db.SaveChangesAsync(ct);

            var rolePermissions = allPermissions
                .Where(p => !p.Module.Equals("Tenants", StringComparison.OrdinalIgnoreCase))
                .Select(p => new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id });
            await db.RolePermissions.AddRangeAsync(rolePermissions, ct);

            var adminUser = new User
            {
                TenantId = tenant.Id,
                Username = request.AdminUsername,
                Email = request.Email,
                PasswordHash = passwordHasher.Hash(request.AdminPassword),
                IsActive = true
            };
            db.Users.Add(adminUser);
            await db.SaveChangesAsync(ct);

            db.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id, TenantId = tenant.Id });
            await db.SaveChangesAsync(ct);
        }

        return new TenantDto(tenant.Id, tenant.Name, tenant.Slug, tenant.Email, tenant.Phone, tenant.Address, tenant.Description, tenant.LogoUrl, tenant.Status, tenant.ExpiresAt, tenant.CreatedAt);
    }
}
