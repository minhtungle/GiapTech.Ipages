using GiapTech.Ipages.Application.ProductCategories.Commands;
using GiapTech.Ipages.Application.ProductCategories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/product-categories")]
public class ProductCategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public ProductCategoriesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
    {
        var result = await _sender.Send(new GetProductCategoriesQuery(activeOnly));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCategoryRequest request)
    {
        var result = await _sender.Send(new UpdateProductCategoryCommand(id, request.Name, request.Description, request.ImageUrl, request.ParentId, request.SortOrder, request.IsActive));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteProductCategoryCommand(id));
        return NoContent();
    }
}

public record UpdateProductCategoryRequest(string Name, string? Description, string? ImageUrl, Guid? ParentId, int SortOrder, bool IsActive);
