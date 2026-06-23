using FluentValidation;
using GiapTech.Ipages.Application.ApiKeys.Queries;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace GiapTech.Ipages.Application.ApiKeys.Commands;

// ── Generate ──────────────────────────────────────────────────────────────────

public record GenerateApiKeyCommand(string Name, string? AllowedOrigins, string? Permissions, DateTime? ExpiresAt) : IRequest<ApiKeyDto>;

public class GenerateApiKeyCommandValidator : AbstractValidator<GenerateApiKeyCommand>
{
    public GenerateApiKeyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class GenerateApiKeyCommandHandler : IRequestHandler<GenerateApiKeyCommand, ApiKeyDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GenerateApiKeyCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant) { _db = db; _tenant = tenant; }

    public async Task<ApiKeyDto> Handle(GenerateApiKeyCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null) throw new ForbiddenException();

        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)).Replace("+", "-").Replace("/", "_").TrimEnd('=');

        var apiKey = new ApiKey
        {
            TenantId = _tenant.TenantId.Value,
            Name = request.Name,
            Key = key,
            Secret = secret,
            AllowedOrigins = request.AllowedOrigins,
            Permissions = request.Permissions,
            ExpiresAt = request.ExpiresAt
        };

        _db.ApiKeys.Add(apiKey);
        await _db.SaveChangesAsync(ct);

        return new ApiKeyDto(apiKey.Id, apiKey.Name, apiKey.Key, apiKey.AllowedOrigins, apiKey.Permissions, apiKey.IsActive, apiKey.ExpiresAt, apiKey.LastUsedAt, apiKey.RequestCount, apiKey.CreatedAt);
    }
}

// ── Revoke ────────────────────────────────────────────────────────────────────

public record RevokeApiKeyCommand(Guid Id) : IRequest;

public class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand>
{
    private readonly IApplicationDbContext _db;

    public RevokeApiKeyCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(RevokeApiKeyCommand request, CancellationToken ct)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ApiKey), request.Id);

        key.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }
}

// ── Delete ────────────────────────────────────────────────────────────────────

public record DeleteApiKeyCommand(Guid Id) : IRequest;

public class DeleteApiKeyCommandHandler : IRequestHandler<DeleteApiKeyCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteApiKeyCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteApiKeyCommand request, CancellationToken ct)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ApiKey), request.Id);

        _db.ApiKeys.Remove(key);
        await _db.SaveChangesAsync(ct);
    }
}
