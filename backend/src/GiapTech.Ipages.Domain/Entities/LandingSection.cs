using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class LandingSection : TenantEntity
{
    public Guid LandingPageId { get; set; }
    public LandingPage LandingPage { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Settings { get; set; }
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}
