using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Articles.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Articles.Commands;

public record UpdateArticleCommand(
    Guid Id,
    string Title,
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

public class UpdateArticleCommandValidator : AbstractValidator<UpdateArticleCommand>
{
    public UpdateArticleCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty();
    }
}

public class UpdateArticleCommandHandler : IRequestHandler<UpdateArticleCommand, ArticleDetailDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateArticleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ArticleDetailDto> Handle(UpdateArticleCommand request, CancellationToken ct)
    {
        var article = await _db.Articles
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Article), request.Id);

        var wasPublished = article.Status == ArticleStatus.Published;

        article.Title = request.Title;
        article.Excerpt = request.Excerpt;
        article.Content = request.Content;
        article.ThumbnailUrl = request.ThumbnailUrl;
        article.CategoryId = request.CategoryId;
        article.Status = request.Status;
        article.ScheduledAt = request.ScheduledAt;
        article.MetaTitle = request.MetaTitle;
        article.MetaDescription = request.MetaDescription;
        article.CanonicalUrl = request.CanonicalUrl;
        article.OgImage = request.OgImage;

        if (!wasPublished && request.Status == ArticleStatus.Published)
            article.PublishedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return ArticleMapping.MapToDetail(article);
    }
}

public record DeleteArticleCommand(Guid Id) : IRequest;

public class DeleteArticleCommandHandler : IRequestHandler<DeleteArticleCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteArticleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteArticleCommand request, CancellationToken ct)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Article), request.Id);

        _db.Articles.Remove(article);
        await _db.SaveChangesAsync(ct);
    }
}

public record PublishArticleCommand(Guid Id) : IRequest<ArticleDetailDto>;

public class PublishArticleCommandHandler : IRequestHandler<PublishArticleCommand, ArticleDetailDto>
{
    private readonly IApplicationDbContext _db;

    public PublishArticleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ArticleDetailDto> Handle(PublishArticleCommand request, CancellationToken ct)
    {
        var article = await _db.Articles
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Article), request.Id);

        article.Status = ArticleStatus.Published;
        article.PublishedAt ??= DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return ArticleMapping.MapToDetail(article);
    }
}
