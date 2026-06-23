using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Seo.Commands;

public record SeoMetadataDto(Guid Id, string EntityType, Guid EntityId, string? MetaTitle, string? MetaDescription, string? CanonicalUrl, string? OgTitle, string? OgDescription, string? OgImage, string? JsonLd);

public record UpsertSeoMetadataCommand(string EntityType, Guid EntityId, string? MetaTitle, string? MetaDescription, string? CanonicalUrl, string? OgTitle, string? OgDescription, string? OgImage, string? JsonLd) : IRequest<SeoMetadataDto>;

public class UpsertSeoMetadataCommandHandler : IRequestHandler<UpsertSeoMetadataCommand, SeoMetadataDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public UpsertSeoMetadataCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant) { _db = db; _tenant = tenant; }

    public async Task<SeoMetadataDto> Handle(UpsertSeoMetadataCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null) throw new ForbiddenException();

        var seo = await _db.SeoMetadata.FirstOrDefaultAsync(s => s.EntityType == request.EntityType && s.EntityId == request.EntityId, ct);

        if (seo == null)
        {
            seo = new SeoMetadata { TenantId = _tenant.TenantId.Value, EntityType = request.EntityType, EntityId = request.EntityId };
            _db.SeoMetadata.Add(seo);
        }

        seo.MetaTitle = request.MetaTitle;
        seo.MetaDescription = request.MetaDescription;
        seo.CanonicalUrl = request.CanonicalUrl;
        seo.OgTitle = request.OgTitle;
        seo.OgDescription = request.OgDescription;
        seo.OgImage = request.OgImage;
        seo.JsonLd = request.JsonLd;

        await _db.SaveChangesAsync(ct);
        return new SeoMetadataDto(seo.Id, seo.EntityType, seo.EntityId, seo.MetaTitle, seo.MetaDescription, seo.CanonicalUrl, seo.OgTitle, seo.OgDescription, seo.OgImage, seo.JsonLd);
    }
}

public record GetSeoMetadataQuery(string EntityType, Guid EntityId) : IRequest<SeoMetadataDto?>;

public class GetSeoMetadataQueryHandler : IRequestHandler<GetSeoMetadataQuery, SeoMetadataDto?>
{
    private readonly IApplicationDbContext _db;

    public GetSeoMetadataQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<SeoMetadataDto?> Handle(GetSeoMetadataQuery request, CancellationToken ct)
    {
        var seo = await _db.SeoMetadata.AsNoTracking().FirstOrDefaultAsync(s => s.EntityType == request.EntityType && s.EntityId == request.EntityId, ct);
        if (seo == null) return null;

        return new SeoMetadataDto(seo.Id, seo.EntityType, seo.EntityId, seo.MetaTitle, seo.MetaDescription, seo.CanonicalUrl, seo.OgTitle, seo.OgDescription, seo.OgImage, seo.JsonLd);
    }
}
