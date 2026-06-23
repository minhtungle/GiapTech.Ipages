using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Auth.Commands;

public record LoginCommand(string Username, string Password, string? TenantSlug) : IRequest<LoginResult>;

public record LoginResult(string AccessToken, string RefreshToken, string TokenType, int ExpiresIn, UserInfo User);
public record UserInfo(Guid Id, string Username, string? FullName, string? Email, bool IsHostAdmin, Guid? TenantId, IEnumerable<string> Permissions);

public class LoginCommandHandler(
    IApplicationDbContext db,
    IJwtService jwtService,
    ICurrentTenantService tenantService) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var query = db.Users.AsQueryable();

        if (!string.IsNullOrEmpty(request.TenantSlug))
        {
            var tenant = await db.Tenants
                .FirstOrDefaultAsync(t => t.Slug == request.TenantSlug, cancellationToken)
                ?? throw new UnauthorizedException("Tenant not found.");

            if (tenant.Status != Domain.Entities.TenantStatus.Active)
                throw new UnauthorizedException("Tenant is not active.");

            query = query.Where(u => u.TenantId == tenant.Id);
        }
        else
        {
            query = query.Where(u => u.IsHostAdmin);
        }

        var user = await query
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, cancellationToken)
            ?? throw new UnauthorizedException("Invalid username or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid username or password.");

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var accessToken = jwtService.GenerateAccessToken(user, permissions);
        var refreshToken = jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);
        user.LastLoginAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            accessToken,
            refreshToken,
            "Bearer",
            3600,
            new UserInfo(user.Id, user.Username, user.FullName, user.Email, user.IsHostAdmin, user.TenantId, permissions));
    }
}
