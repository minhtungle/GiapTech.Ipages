using GiapTech.Ipages.Domain.Common;

namespace GiapTech.Ipages.Domain.Entities;

public class MediaFile : TenantEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Url { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public MediaFileType Type { get; set; }
    public string? Folder { get; set; }
    public string? Tags { get; set; }
    public string? AltText { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public Guid UploadedBy { get; set; }
}

public enum MediaFileType
{
    Image = 1,
    Video = 2,
    Document = 3,
    Other = 4
}
