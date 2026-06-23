using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class Form : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FormType Type { get; set; } = FormType.Contact;
    public string Fields { get; set; } = "[]";
    public string? SuccessMessage { get; set; }
    public string? NotifyEmails { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FormEntry> Entries { get; set; } = [];
}

public enum FormType
{
    Contact = 1,
    Lead = 2,
    Survey = 3
}
