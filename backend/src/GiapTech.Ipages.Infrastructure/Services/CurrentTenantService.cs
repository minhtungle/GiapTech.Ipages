using GiapTech.Ipages.Application.Common.Interfaces;

namespace GiapTech.Ipages.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    public Guid? TenantId { get; private set; }
    public string? TenantSlug { get; private set; }
    public bool IsHostAdmin { get; private set; }

    public void SetTenant(Guid tenantId, string slug)
    {
        TenantId = tenantId;
        TenantSlug = slug;
        IsHostAdmin = false;
    }

    public void SetHostAdmin()
    {
        TenantId = null;
        TenantSlug = null;
        IsHostAdmin = true;
    }
}
