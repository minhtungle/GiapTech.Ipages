using GiapTech.Ipages.Application.LandingPages.Commands;
using GiapTech.Ipages.Application.LandingPages.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/landing-pages")]
public class LandingPagesController : ControllerBase
{
    private readonly ISender _sender;

    public LandingPagesController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new GetLandingPagesQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _sender.Send(new GetLandingPageBySlugQuery(slug));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateLandingPageCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLandingPageRequest request)
    {
        var result = await _sender.Send(new UpdateLandingPageCommand(id, request.Name, request.Template, request.Status, request.MetaTitle, request.MetaDescription, request.OgImage));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteLandingPageCommand(id));
        return NoContent();
    }

    [HttpPut("{pageId:guid}/sections")]
    [Authorize]
    public async Task<IActionResult> UpsertSection(Guid pageId, [FromBody] UpsertSectionRequest request)
    {
        var result = await _sender.Send(new UpsertLandingSectionCommand(request.Id, pageId, request.Type, request.Title, request.Settings, request.SortOrder, request.IsVisible));
        return Ok(result);
    }

    [HttpDelete("sections/{sectionId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteSection(Guid sectionId)
    {
        await _sender.Send(new DeleteLandingSectionCommand(sectionId));
        return NoContent();
    }
}

public record UpdateLandingPageRequest(string Name, string? Template, LandingPageStatus Status, string? MetaTitle, string? MetaDescription, string? OgImage);
public record UpsertSectionRequest(Guid? Id, string Type, string? Title, string? Settings, int SortOrder, bool IsVisible);
