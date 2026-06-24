using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class ProductVariant : TenantEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? Weight { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? AttributeValues { get; set; }  // JSON: {"Màu":"Đỏ","Size":"XL"}
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
