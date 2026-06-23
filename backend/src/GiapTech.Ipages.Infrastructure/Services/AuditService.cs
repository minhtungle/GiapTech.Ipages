using System.Text.Json;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace GiapTech.Ipages.Infrastructure.Services;

public class AuditService(
    ApplicationDbContext db,
    ICurrentUserService currentUser,
    ICurrentTenantService currentTenant,
    IHttpContextAccessor httpContextAccessor) : IAuditService
{
    public async Task LogAsync(string action, string entityType, string? entityId = null,
        object? oldValues = null, object? newValues = null,
        bool isSuccess = true, string? errorMessage = null, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = currentTenant.TenantId,
            UserId = currentUser.UserId,
            Username = currentUser.Username,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage
        };

        await db.AuditLogs.AddAsync(log, ct);
        await db.SaveChangesAsync(ct);
    }
}
