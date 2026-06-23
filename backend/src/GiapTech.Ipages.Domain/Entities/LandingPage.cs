using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class LandingPage : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Template { get; set; }
    public LandingPageStatus Status { get; set; } = LandingPageStatus.Draft;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImage { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<LandingSection> Sections { get; set; } = [];
}

public enum LandingPageStatus
{
    Draft = 1,
    Published = 2,
    Archived = 3
}
