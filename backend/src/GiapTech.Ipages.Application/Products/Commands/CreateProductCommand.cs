using FluentValidation;
using GiapTech.Ipages.Application.Common.Exceptions;
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

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$").WithMessage("Slug chỉ chứa chữ thường, số và dấu gạch ngang.");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0).LessThan(x => x.Price).When(x => x.SalePrice.HasValue);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDetailDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateProductCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<ProductDetailDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        if (_tenant.TenantId == null)
            throw new ForbiddenException();

        var slugExists = await _db.Products.AnyAsync(p => p.Slug == request.Slug, ct);
        if (slugExists)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Slug", "Slug đã tồn tại.") });

        var product = new Product
        {
            TenantId = _tenant.TenantId.Value,
            Name = request.Name,
            Slug = request.Slug,
            Sku = request.Sku,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            Price = request.Price,
            SalePrice = request.SalePrice,
            ThumbnailUrl = request.ThumbnailUrl,
            Images = request.Images,
            CategoryId = request.CategoryId,
            StockQuantity = request.StockQuantity,
            TrackInventory = request.TrackInventory,
            Status = request.Status,
            SortOrder = request.SortOrder,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            CanonicalUrl = request.CanonicalUrl
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return ProductMapping.MapToDetail(product);
    }
}
