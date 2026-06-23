using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Articles.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Articles.Commands;

public record CreateArticleCommand(
    string Title,
    string Slug,
    string? Excerpt,
    string Content,
    string? ThumbnailUrl,
    Guid? CategoryId,
    ArticleStatus Status,
    DateTime? ScheduledAt,
    string? MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    string? OgImage) : IRequest<ArticleDetailDto>;

public class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(500).Matches("^[a-z0-9-]+$").WithMessage("Slug không hợp lệ.");
        RuleFor(x => x.Content).NotEmpty();
    }
}

public class CreateArticleCommandHandler : IRequestHandler<CreateArticleCommand, ArticleDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentUserService _currentUser;

    public CreateArticleCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentUserService currentUser)
    {
        _db = db;
        _tenant = tenant;
        _currentUser = currentUser;
    }

    public async Task<ArticleDetailDto> Handle(CreateArticleCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null)
            throw new ForbiddenException();

        var slugExists = await _db.Articles.AnyAsync(a => a.Slug == request.Slug, ct);
        if (slugExists)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Slug", "Slug đã tồn tại.") });

        var article = new Article
        {
            TenantId = _tenant.TenantId.Value,
            Title = request.Title,
            Slug = request.Slug,
            Excerpt = request.Excerpt,
            Content = request.Content,
            ThumbnailUrl = request.ThumbnailUrl,
            CategoryId = request.CategoryId,
            AuthorId = _currentUser.UserId ?? Guid.Empty,
            Status = request.Status,
            PublishedAt = request.Status == ArticleStatus.Published ? DateTime.UtcNow : null,
            ScheduledAt = request.ScheduledAt,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            CanonicalUrl = request.CanonicalUrl,
            OgImage = request.OgImage
        };

        _db.Articles.Add(article);
        await _db.SaveChangesAsync(ct);

        return ArticleMapping.MapToDetail(article);
    }
}
