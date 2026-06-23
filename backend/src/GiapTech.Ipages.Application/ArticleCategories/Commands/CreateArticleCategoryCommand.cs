using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.ArticleCategories.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.ArticleCategories.Commands;

public record CreateArticleCategoryCommand(
    string Name,
    string Slug,
    string? Description,
    Guid? ParentId,
    int SortOrder) : IRequest<ArticleCategoryDto>;

public class CreateArticleCategoryCommandValidator : AbstractValidator<CreateArticleCategoryCommand>
{
    public CreateArticleCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$").WithMessage("Slug không hợp lệ.");
    }
}

public class CreateArticleCategoryCommandHandler : IRequestHandler<CreateArticleCategoryCommand, ArticleCategoryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateArticleCategoryCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<ArticleCategoryDto> Handle(CreateArticleCategoryCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null)
            throw new ForbiddenException();

        var slugExists = await _db.ArticleCategories.AnyAsync(c => c.Slug == request.Slug, ct);
        if (slugExists)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Slug", "Slug đã tồn tại.") });

        var category = new ArticleCategory
        {
            TenantId = _tenant.TenantId.Value,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder
        };

        _db.ArticleCategories.Add(category);
        await _db.SaveChangesAsync(ct);

        return new ArticleCategoryDto(category.Id, category.Name, category.Slug, category.Description, category.ParentId, null, category.SortOrder, category.IsActive, 0);
    }
}

public record UpdateArticleCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentId,
    int SortOrder,
    bool IsActive) : IRequest<ArticleCategoryDto>;

public class UpdateArticleCategoryCommandValidator : AbstractValidator<UpdateArticleCategoryCommand>
{
    public UpdateArticleCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateArticleCategoryCommandHandler : IRequestHandler<UpdateArticleCategoryCommand, ArticleCategoryDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateArticleCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ArticleCategoryDto> Handle(UpdateArticleCategoryCommand request, CancellationToken ct)
    {
        var category = await _db.ArticleCategories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ArticleCategory), request.Id);

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentId = request.ParentId;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);

        return new ArticleCategoryDto(category.Id, category.Name, category.Slug, category.Description, category.ParentId, null, category.SortOrder, category.IsActive, 0);
    }
}
