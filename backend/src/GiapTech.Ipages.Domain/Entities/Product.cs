using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class Product : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    // Giá
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public DateTime? SalePriceFrom { get; set; }
    public DateTime? SalePriceTo { get; set; }
    public decimal? CostPerItem { get; set; }

    // Media
    public string? ThumbnailUrl { get; set; }
    public string? Images { get; set; }         // JSON: [{url,alt,order}]
    public string? VideoUrl { get; set; }

    // Phân loại
    public Guid? CategoryId { get; set; }
    public ProductCategory? Category { get; set; }
    public string? TagsJson { get; set; }        // JSON: ["tag1","tag2"]

    // Kho & hiển thị
    public int StockQuantity { get; set; }
    public bool TrackInventory { get; set; } = true;
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
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
    Draft = 1,
    Hidden = 2,
    Active = 3,
    OutOfStock = 4,
    Discontinued = 5
}
