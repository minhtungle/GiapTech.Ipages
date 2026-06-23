namespace GiapTech.Ipages.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string? entityId = null,
        object? oldValues = null, object? newValues = null,
        bool isSuccess = true, string? errorMessage = null,
        CancellationToken ct = default);
}
