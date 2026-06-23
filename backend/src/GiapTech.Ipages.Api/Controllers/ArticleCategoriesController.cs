using GiapTech.Ipages.Application.ArticleCategories.Commands;
using GiapTech.Ipages.Application.ArticleCategories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/article-categories")]
public class ArticleCategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public ArticleCategoriesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
    {
        var result = await _sender.Send(new GetArticleCategoriesQuery(activeOnly));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateArticleCategoryCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleCategoryRequest request)
    {
        var result = await _sender.Send(new UpdateArticleCategoryCommand(id, request.Name, request.Description, request.ParentId, request.SortOrder, request.IsActive));
        return Ok(result);
    }
}

public record UpdateArticleCategoryRequest(string Name, string? Description, Guid? ParentId, int SortOrder, bool IsActive);
