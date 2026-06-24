using GiapTech.Ipages.Application.Products.Commands;
using GiapTech.Ipages.Application.Products.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] Guid? categoryId = null, [FromQuery] ProductStatus? status = null)
    {
        var result = await _sender.Send(new GetProductsQuery(page, pageSize, search, categoryId, status));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetProductByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _sender.Send(new GetProductBySlugQuery(slug));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _sender.Send(new CreateProductCommand(
            request.Name, request.Slug, request.Sku, request.ShortDescription, request.Description,
            request.Price, request.SalePrice, request.SalePriceFrom, request.SalePriceTo, request.CostPerItem,
            request.ThumbnailUrl, request.Images, request.VideoUrl,
            request.CategoryId, request.TagsJson,
            request.StockQuantity, request.TrackInventory, request.Status,
            request.SortOrder, request.MetaTitle, request.MetaDescription, request.CanonicalUrl));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _sender.Send(new UpdateProductCommand(id, request.Name, request.Sku, request.ShortDescription, request.Description, request.Price, request.SalePrice, request.SalePriceFrom, request.SalePriceTo, request.CostPerItem, request.ThumbnailUrl, request.Images, request.VideoUrl, request.CategoryId, request.TagsJson, request.StockQuantity, request.TrackInventory, request.Status, request.SortOrder, request.MetaTitle, request.MetaDescription, request.CanonicalUrl));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteProductCommand(id));
        return NoContent();
    }

    [HttpPatch("{id:guid}/inventory")]
    [Authorize]
    public async Task<IActionResult> UpdateInventory(Guid id, [FromBody] UpdateInventoryRequest request)
    {
        await _sender.Send(new UpdateInventoryCommand(id, request.Quantity, request.Note));
        return NoContent();
    }
}

public record CreateProductRequest(
    string Name, string Slug, string? Sku, string? ShortDescription, string? Description,
    decimal Price, decimal? SalePrice, DateTime? SalePriceFrom, DateTime? SalePriceTo, decimal? CostPerItem,
    string? ThumbnailUrl, string? Images, string? VideoUrl,
    Guid? CategoryId, string? TagsJson,
    int StockQuantity, bool TrackInventory, ProductStatus Status,
    int SortOrder, string? MetaTitle, string? MetaDescription, string? CanonicalUrl);

public record UpdateProductRequest(
    string Name, string? Sku, string? ShortDescription, string? Description,
    decimal Price, decimal? SalePrice, DateTime? SalePriceFrom, DateTime? SalePriceTo, decimal? CostPerItem,
    string? ThumbnailUrl, string? Images, string? VideoUrl,
    Guid? CategoryId, string? TagsJson,
    int StockQuantity, bool TrackInventory, ProductStatus Status,
    int SortOrder, string? MetaTitle, string? MetaDescription, string? CanonicalUrl);

public record UpdateInventoryRequest(int Quantity, string? Note);
