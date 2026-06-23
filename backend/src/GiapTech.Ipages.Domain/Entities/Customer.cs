using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class Customer : TenantEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool IsActive { get; set; } = true;
    public int LoyaltyPoints { get; set; }
    public string? Notes { get; set; }

    public ICollection<CustomerAddress> Addresses { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
