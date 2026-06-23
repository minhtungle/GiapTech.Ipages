using GiapTech.Ipages.Application.Articles.Commands;
using GiapTech.Ipages.Application.Articles.Queries;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/articles")]
public class ArticlesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUser;

    public ArticlesController(ISender sender, ICurrentUserService currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] Guid? categoryId = null, [FromQuery] ArticleStatus? status = null)
    {
        var publishedOnly = !_currentUser.IsAuthenticated;
        var result = await _sender.Send(new GetArticlesQuery(page, pageSize, search, categoryId, status, publishedOnly));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetArticleByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _sender.Send(new GetArticleBySlugQuery(slug));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateArticleCommand command)
    {
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleRequest request)
    {
        var result = await _sender.Send(new UpdateArticleCommand(id, request.Title, request.Excerpt, request.Content, request.ThumbnailUrl, request.CategoryId, request.Status, request.ScheduledAt, request.MetaTitle, request.MetaDescription, request.CanonicalUrl, request.OgImage));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteArticleCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize]
    public async Task<IActionResult> Publish(Guid id)
    {
        var result = await _sender.Send(new PublishArticleCommand(id));
        return Ok(result);
    }
}

public record UpdateArticleRequest(
    string Title, string? Excerpt, string Content, string? ThumbnailUrl,
    Guid? CategoryId, ArticleStatus Status, DateTime? ScheduledAt,
    string? MetaTitle, string? MetaDescription, string? CanonicalUrl, string? OgImage);
