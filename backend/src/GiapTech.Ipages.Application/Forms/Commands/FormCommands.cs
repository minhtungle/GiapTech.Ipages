using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Forms.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Forms.Commands;

// ── Create Form ───────────────────────────────────────────────────────────────

public record CreateFormCommand(string Name, string? Description, FormType Type, string Fields, string? SuccessMessage, string? NotifyEmails) : IRequest<FormDto>;

public class CreateFormCommandValidator : AbstractValidator<CreateFormCommand>
{
    public CreateFormCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Fields).NotEmpty();
    }
}

public class CreateFormCommandHandler : IRequestHandler<CreateFormCommand, FormDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateFormCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant) { _db = db; _tenant = tenant; }

    public async Task<FormDto> Handle(CreateFormCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null) throw new ForbiddenException();

        var form = new Form { TenantId = _tenant.TenantId.Value, Name = request.Name, Description = request.Description, Type = request.Type, Fields = request.Fields, SuccessMessage = request.SuccessMessage, NotifyEmails = request.NotifyEmails };
        _db.Forms.Add(form);
        await _db.SaveChangesAsync(ct);

        return new FormDto(form.Id, form.Name, form.Description, form.Type, form.Fields, form.SuccessMessage, form.NotifyEmails, form.IsActive, 0, form.CreatedAt);
    }
}

// ── Update Form ───────────────────────────────────────────────────────────────

public record UpdateFormCommand(Guid Id, string Name, string? Description, string Fields, string? SuccessMessage, string? NotifyEmails, bool IsActive) : IRequest<FormDto>;

public class UpdateFormCommandHandler : IRequestHandler<UpdateFormCommand, FormDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateFormCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<FormDto> Handle(UpdateFormCommand request, CancellationToken ct)
    {
        var form = await _db.Forms.FirstOrDefaultAsync(f => f.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Form), request.Id);

        form.Name = request.Name;
        form.Description = request.Description;
        form.Fields = request.Fields;
        form.SuccessMessage = request.SuccessMessage;
        form.NotifyEmails = request.NotifyEmails;
        form.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return new FormDto(form.Id, form.Name, form.Description, form.Type, form.Fields, form.SuccessMessage, form.NotifyEmails, form.IsActive, 0, form.CreatedAt);
    }
}

// ── Delete Form ───────────────────────────────────────────────────────────────

public record DeleteFormCommand(Guid Id) : IRequest;

public class DeleteFormCommandHandler : IRequestHandler<DeleteFormCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteFormCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteFormCommand request, CancellationToken ct)
    {
        var form = await _db.Forms.FirstOrDefaultAsync(f => f.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Form), request.Id);

        _db.Forms.Remove(form);
        await _db.SaveChangesAsync(ct);
    }
}

// ── Submit Entry ──────────────────────────────────────────────────────────────

public record SubmitFormEntryCommand(Guid FormId, string Data) : IRequest<FormEntryDto>;

public class SubmitFormEntryCommandHandler : IRequestHandler<SubmitFormEntryCommand, FormEntryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly IHttpContextAccessor _httpContext;

    public SubmitFormEntryCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, IHttpContextAccessor httpContext)
    {
        _db = db; _tenant = tenant; _httpContext = httpContext;
    }

    public async Task<FormEntryDto> Handle(SubmitFormEntryCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null) throw new NotFoundException(nameof(Form), request.FormId);

        var form = await _db.Forms.FirstOrDefaultAsync(f => f.Id == request.FormId && f.IsActive, ct)
            ?? throw new NotFoundException(nameof(Form), request.FormId);

        var ip = _httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var ua = _httpContext.HttpContext?.Request.Headers["User-Agent"].ToString();

        var entry = new FormEntry { TenantId = _tenant.TenantId.Value, FormId = form.Id, Data = request.Data, IpAddress = ip, UserAgent = ua };
        _db.FormEntries.Add(entry);
        await _db.SaveChangesAsync(ct);

        return new FormEntryDto(entry.Id, entry.FormId, entry.Data, entry.IpAddress, entry.IsRead, entry.CreatedAt);
    }
}

// ── Mark Entry Read ───────────────────────────────────────────────────────────

public record MarkEntryReadCommand(Guid EntryId, bool IsRead = true) : IRequest;

public class MarkEntryReadCommandHandler : IRequestHandler<MarkEntryReadCommand>
{
    private readonly IApplicationDbContext _db;

    public MarkEntryReadCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(MarkEntryReadCommand request, CancellationToken ct)
    {
        var entry = await _db.FormEntries.FirstOrDefaultAsync(e => e.Id == request.EntryId, ct)
            ?? throw new NotFoundException(nameof(FormEntry), request.EntryId);

        entry.IsRead = request.IsRead;
        await _db.SaveChangesAsync(ct);
    }
}
