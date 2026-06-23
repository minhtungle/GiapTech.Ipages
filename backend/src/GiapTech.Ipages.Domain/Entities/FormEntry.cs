using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class FormEntry : TenantEntity
{
    public Guid FormId { get; set; }
    public Form Form { get; set; } = null!;
    public string Data { get; set; } = "{}";
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsRead { get; set; }
}
