using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class MenuItem : TenantEntity
{
    public Guid MenuId { get; set; }
    public Menu Menu { get; set; } = null!;
    public string Label { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Target { get; set; }
    public string? Icon { get; set; }
    public Guid? ParentId { get; set; }
    public MenuItem? Parent { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MenuItem> Children { get; set; } = [];
}
