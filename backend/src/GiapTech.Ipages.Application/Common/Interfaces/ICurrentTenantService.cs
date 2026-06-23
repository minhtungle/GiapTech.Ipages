namespace GiapTech.Ipages.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    bool IsHostAdmin { get; }
    void SetTenant(Guid tenantId, string slug);
    void SetHostAdmin();
}
