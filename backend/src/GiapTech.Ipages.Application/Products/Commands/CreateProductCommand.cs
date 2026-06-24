using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using ValidationException = GiapTech.Ipages.Application.Common.Exceptions.ValidationException;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Products.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Products.Commands;

public record CreateProductCommand(
    string Name,
    string Slug,
    string? Sku,
    string? ShortDescription,
    string? Description,
    decimal Price,
    decimal? SalePrice,
    DateTime? SalePriceFrom,
    DateTime? SalePriceTo,
    decimal? CostPerItem,
    string? ThumbnailUrl,
    string? Images,
    string? VideoUrl,
    Guid? CategoryId,
    string? TagsJson,
    int StockQuantity,
    bool TrackInventory,
    ProductStatus Status,
    int SortOrder,
    string? MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl) : IRequest<ProductDetailDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$").WithMessage("Slug chỉ chứa chữ thường, số và dấu gạch ngang.");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0).LessThan(x => x.Price).When(x => x.SalePrice.HasValue);
        RuleFor(x => x.CostPerItem).GreaterThanOrEqualTo(0).When(x => x.CostPerItem.HasValue);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePriceTo).GreaterThan(x => x.SalePriceFrom).When(x => x.SalePriceFrom.HasValue && x.SalePriceTo.HasValue);
    }
}

public class CreateProductCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    : IRequestHandler<CreateProductCommand, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        if (tenant.TenantId == null)
            throw new ForbiddenException();

        var slugExists = await db.Products.AnyAsync(p => p.Slug == request.Slug, ct);
        if (slugExists)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Slug", "Slug đã tồn tại.") });

        var product = new Product
        {
            TenantId = tenant.TenantId.Value,
            Name = request.Name,
            Slug = request.Slug,
            Sku = request.Sku,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            Price = request.Price,
            SalePrice = request.SalePrice,
            SalePriceFrom = request.SalePriceFrom,
            SalePriceTo = request.SalePriceTo,
            CostPerItem = request.CostPerItem,
            ThumbnailUrl = request.ThumbnailUrl,
            Images = request.Images,
            VideoUrl = request.VideoUrl,
            CategoryId = request.CategoryId,
            TagsJson = request.TagsJson,
            StockQuantity = request.StockQuantity,
            TrackInventory = request.TrackInventory,
            Status = request.Status,
            SortOrder = request.SortOrder,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            CanonicalUrl = request.CanonicalUrl
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        return ProductMapping.MapToDetail(product);
    }
}
