using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class Cart : TenantEntity
{
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? SessionId { get; set; }
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public ICollection<CartItem> Items { get; set; } = [];
}
