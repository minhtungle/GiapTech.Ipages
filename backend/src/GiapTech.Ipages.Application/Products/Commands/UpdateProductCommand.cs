using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Products.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiapTech.Ipages.Application.Products.Commands;

public record UpdateProductCommand(
    Guid Id,
    string Name,
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

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0).LessThan(x => x.Price).When(x => x.SalePrice.HasValue);
        RuleFor(x => x.CostPerItem).GreaterThanOrEqualTo(0).When(x => x.CostPerItem.HasValue);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePriceTo).GreaterThan(x => x.SalePriceFrom).When(x => x.SalePriceFrom.HasValue && x.SalePriceTo.HasValue);
    }
}

public class UpdateProductCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateProductCommand, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.Name = request.Name;
        product.Sku = request.Sku;
        product.ShortDescription = request.ShortDescription;
        product.Description = request.Description;
        product.Price = request.Price;
        product.SalePrice = request.SalePrice;
        product.SalePriceFrom = request.SalePriceFrom;
        product.SalePriceTo = request.SalePriceTo;
        product.CostPerItem = request.CostPerItem;
        product.ThumbnailUrl = request.ThumbnailUrl;
        product.Images = request.Images;
        product.VideoUrl = request.VideoUrl;
        product.CategoryId = request.CategoryId;
        product.TagsJson = request.TagsJson;
        product.StockQuantity = request.StockQuantity;
        product.TrackInventory = request.TrackInventory;
        product.Status = request.Status;
        product.SortOrder = request.SortOrder;
        product.MetaTitle = request.MetaTitle;
        product.MetaDescription = request.MetaDescription;
        product.CanonicalUrl = request.CanonicalUrl;

        await db.SaveChangesAsync(ct);

        return ProductMapping.MapToDetail(product);
    }
}
