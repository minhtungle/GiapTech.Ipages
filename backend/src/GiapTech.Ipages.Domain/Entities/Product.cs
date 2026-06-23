using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class Product : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Images { get; set; }
    public Guid? CategoryId { get; set; }
    public ProductCategory? Category { get; set; }
    public int StockQuantity { get; set; }
    public bool TrackInventory { get; set; } = true;
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public int SortOrder { get; set; }
    public int SoldCount { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }

    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<ProductAttribute> Attributes { get; set; } = [];
    public ICollection<Inventory> Inventories { get; set; } = [];
}

public enum ProductStatus
{
    Active = 1,
    Inactive = 2,
    OutOfStock = 3,
    Discontinued = 4
}
