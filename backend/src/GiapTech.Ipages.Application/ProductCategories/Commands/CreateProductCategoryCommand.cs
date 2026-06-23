using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using ValidationException = GiapTech.Ipages.Application.Common.Exceptions.ValidationException;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.ProductCategories.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.ProductCategories.Commands;

public record CreateProductCategoryCommand(
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    Guid? ParentId,
    int SortOrder) : IRequest<ProductCategoryDto>;

public class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$").WithMessage("Slug không hợp lệ.");
    }
}

public class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand, ProductCategoryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateProductCategoryCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<ProductCategoryDto> Handle(CreateProductCategoryCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null)
            throw new ForbiddenException();

        var slugExists = await _db.ProductCategories.AnyAsync(c => c.Slug == request.Slug, ct);
        if (slugExists)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Slug", "Slug đã tồn tại.") });

        var category = new ProductCategory
        {
            TenantId = _tenant.TenantId.Value,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder
        };

        _db.ProductCategories.Add(category);
        await _db.SaveChangesAsync(ct);

        return new ProductCategoryDto(category.Id, category.Name, category.Slug, category.Description, category.ImageUrl, category.ParentId, null, category.SortOrder, category.IsActive, 0);
    }
}

public record UpdateProductCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    Guid? ParentId,
    int SortOrder,
    bool IsActive) : IRequest<ProductCategoryDto>;

public class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateProductCategoryCommandHandler : IRequestHandler<UpdateProductCategoryCommand, ProductCategoryDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateProductCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductCategoryDto> Handle(UpdateProductCategoryCommand request, CancellationToken ct)
    {
        var category = await _db.ProductCategories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.Id);

        category.Name = request.Name;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.ParentId = request.ParentId;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);

        return new ProductCategoryDto(category.Id, category.Name, category.Slug, category.Description, category.ImageUrl, category.ParentId, null, category.SortOrder, category.IsActive, 0);
    }
}

public record DeleteProductCategoryCommand(Guid Id) : IRequest;

public class DeleteProductCategoryCommandHandler : IRequestHandler<DeleteProductCategoryCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteProductCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteProductCategoryCommand request, CancellationToken ct)
    {
        var category = await _db.ProductCategories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.Id);

        _db.ProductCategories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }
}
