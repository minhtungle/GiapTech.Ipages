using GiapTech.Ipages.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace GiapTech.Ipages.Infrastructure.Services;

public class MinioStorageService(IMinioClient minio, IConfiguration configuration) : IStorageService
{
    private readonly string _bucket = configuration["MinioSettings:BucketName"]!;
    private readonly string _host = configuration["MinioSettings:Host"]!;
    private readonly string _port = configuration["MinioSettings:Port"] ?? "9000";
    private readonly bool _useSSL = bool.Parse(configuration["MinioSettings:UseSSL"] ?? "false");

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string? folder = null, CancellationToken ct = default)
    {
        await EnsureBucketExistsAsync(ct);

        var objectName = folder != null ? $"{folder}/{fileName}" : fileName;

        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await minio.PutObjectAsync(args, ct);

        return objectName;
    }

    public async Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storagePath);
        await minio.RemoveObjectAsync(args, ct);
    }

    public string GetPublicUrl(string storagePath)
    {
        var scheme = _useSSL ? "https" : "http";
        return $"{scheme}://{_host}:{_port}/{_bucket}/{storagePath}";
    }

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        var exists = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket), ct);
        if (!exists)
            await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket), ct);
    }
}
