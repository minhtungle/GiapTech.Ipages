using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.LandingPages.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.LandingPages.Commands;

// ── Create LandingPage ────────────────────────────────────────────────────────

public record CreateLandingPageCommand(string Name, string Slug, string? Template, string? MetaTitle, string? MetaDescription, string? OgImage) : IRequest<LandingPageDetailDto>;

public class CreateLandingPageCommandValidator : AbstractValidator<CreateLandingPageCommand>
{
    public CreateLandingPageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$").WithMessage("Slug không hợp lệ.");
    }
}

public class CreateLandingPageCommandHandler : IRequestHandler<CreateLandingPageCommand, LandingPageDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateLandingPageCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant) { _db = db; _tenant = tenant; }

    public async Task<LandingPageDetailDto> Handle(CreateLandingPageCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null) throw new ForbiddenException();

        var slugExists = await _db.LandingPages.AnyAsync(p => p.Slug == request.Slug, ct);
        if (slugExists) throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Slug", "Slug đã tồn tại.") });

        var page = new LandingPage { TenantId = _tenant.TenantId.Value, Name = request.Name, Slug = request.Slug, Template = request.Template, MetaTitle = request.MetaTitle, MetaDescription = request.MetaDescription, OgImage = request.OgImage };
        _db.LandingPages.Add(page);
        await _db.SaveChangesAsync(ct);

        return new LandingPageDetailDto(page.Id, page.Name, page.Slug, page.Template, page.Status, page.MetaTitle, page.MetaDescription, page.OgImage, page.PublishedAt, [], page.CreatedAt);
    }
}

// ── Update LandingPage ────────────────────────────────────────────────────────

public record UpdateLandingPageCommand(Guid Id, string Name, string? Template, LandingPageStatus Status, string? MetaTitle, string? MetaDescription, string? OgImage) : IRequest<LandingPageDetailDto>;

public class UpdateLandingPageCommandHandler : IRequestHandler<UpdateLandingPageCommand, LandingPageDetailDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateLandingPageCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<LandingPageDetailDto> Handle(UpdateLandingPageCommand request, CancellationToken ct)
    {
        var page = await _db.LandingPages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(LandingPage), request.Id);

        var wasPublished = page.Status == LandingPageStatus.Published;
        page.Name = request.Name;
        page.Template = request.Template;
        page.Status = request.Status;
        page.MetaTitle = request.MetaTitle;
        page.MetaDescription = request.MetaDescription;
        page.OgImage = request.OgImage;

        if (!wasPublished && request.Status == LandingPageStatus.Published)
            page.PublishedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new LandingPageDetailDto(page.Id, page.Name, page.Slug, page.Template, page.Status, page.MetaTitle, page.MetaDescription, page.OgImage, page.PublishedAt,
            page.Sections.OrderBy(s => s.SortOrder).Select(s => new LandingSectionDto(s.Id, s.Type, s.Title, s.Settings, s.SortOrder, s.IsVisible)),
            page.CreatedAt);
    }
}

// ── Delete LandingPage ────────────────────────────────────────────────────────

public record DeleteLandingPageCommand(Guid Id) : IRequest;

public class DeleteLandingPageCommandHandler : IRequestHandler<DeleteLandingPageCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteLandingPageCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteLandingPageCommand request, CancellationToken ct)
    {
        var page = await _db.LandingPages.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(LandingPage), request.Id);

        _db.LandingPages.Remove(page);
        await _db.SaveChangesAsync(ct);
    }
}

// ── Upsert Section ────────────────────────────────────────────────────────────

public record UpsertLandingSectionCommand(Guid? Id, Guid LandingPageId, string Type, string? Title, string? Settings, int SortOrder, bool IsVisible) : IRequest<LandingSectionDto>;

public class UpsertLandingSectionCommandHandler : IRequestHandler<UpsertLandingSectionCommand, LandingSectionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public UpsertLandingSectionCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant) { _db = db; _tenant = tenant; }

    public async Task<LandingSectionDto> Handle(UpsertLandingSectionCommand request, CancellationToken ct)
    {
        LandingSection section;

        if (request.Id.HasValue)
        {
            section = await _db.LandingSections.FirstOrDefaultAsync(s => s.Id == request.Id.Value, ct)
                ?? throw new NotFoundException(nameof(LandingSection), request.Id.Value);
        }
        else
        {
            if (_tenant.TenantId == null) throw new ForbiddenException();
            section = new LandingSection { TenantId = _tenant.TenantId.Value, LandingPageId = request.LandingPageId };
            _db.LandingSections.Add(section);
        }

        section.Type = request.Type;
        section.Title = request.Title;
        section.Settings = request.Settings;
        section.SortOrder = request.SortOrder;
        section.IsVisible = request.IsVisible;

        await _db.SaveChangesAsync(ct);
        return new LandingSectionDto(section.Id, section.Type, section.Title, section.Settings, section.SortOrder, section.IsVisible);
    }
}

// ── Delete Section ────────────────────────────────────────────────────────────

public record DeleteLandingSectionCommand(Guid Id) : IRequest;

public class DeleteLandingSectionCommandHandler : IRequestHandler<DeleteLandingSectionCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteLandingSectionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteLandingSectionCommand request, CancellationToken ct)
    {
        var section = await _db.LandingSections.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(LandingSection), request.Id);

        _db.LandingSections.Remove(section);
        await _db.SaveChangesAsync(ct);
    }
}
