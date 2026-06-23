namespace GiapTech.Ipages.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
    string GetPublicUrl(string storagePath);
}
