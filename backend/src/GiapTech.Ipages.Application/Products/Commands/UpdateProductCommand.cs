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
    string? Description,
    string? ShortDescription,
    decimal Price,
    decimal? SalePrice,
    string? ThumbnailUrl,
    string? Images,
    Guid? CategoryId,
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
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDetailDto>
{
    private readonly IApplicationDbContext _db;

    public UpdateProductCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ProductDetailDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.Name = request.Name;
        product.Sku = request.Sku;
        product.Description = request.Description;
        product.ShortDescription = request.ShortDescription;
        product.Price = request.Price;
        product.SalePrice = request.SalePrice;
        product.ThumbnailUrl = request.ThumbnailUrl;
        product.Images = request.Images;
        product.CategoryId = request.CategoryId;
        product.StockQuantity = request.StockQuantity;
        product.TrackInventory = request.TrackInventory;
        product.Status = request.Status;
        product.SortOrder = request.SortOrder;
        product.MetaTitle = request.MetaTitle;
        product.MetaDescription = request.MetaDescription;
        product.CanonicalUrl = request.CanonicalUrl;

        await _db.SaveChangesAsync(ct);

        return ProductMapping.MapToDetail(product);
    }
}
