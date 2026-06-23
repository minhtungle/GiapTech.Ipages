using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public DateTime? ExpiresAt { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<User> Users { get; set; } = [];
}

public enum TenantStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Expired = 4
}
