using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class Inventory : TenantEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid? VariantId { get; set; }
    public ProductVariant? Variant { get; set; }
    public InventoryType Type { get; set; }
    public int Quantity { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedBy { get; set; }
}

public enum InventoryType
{
    Import = 1,
    Export = 2,
    Adjustment = 3,
    Return = 4
}
