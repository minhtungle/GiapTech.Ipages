using GiapTech.Ipages.Application.Menus.Commands;
using GiapTech.Ipages.Application.Menus.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/menus")]
public class MenusController : ControllerBase
{
    private readonly ISender _sender;

    public MenusController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var result = await _sender.Send(new GetMenusQuery());
        return Ok(result);
    }

    [HttpGet("location/{location}")]
    public async Task<IActionResult> GetByLocation(string location)
    {
        var result = await _sender.Send(new GetMenuByLocationQuery(location));
        return result == null ? NoContent() : Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateMenuCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMenuRequest request)
    {
        var result = await _sender.Send(new UpdateMenuCommand(id, request.Name, request.Location, request.IsActive));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteMenuCommand(id));
        return NoContent();
    }

    [HttpPut("{menuId:guid}/items")]
    [Authorize]
    public async Task<IActionResult> UpsertItem(Guid menuId, [FromBody] UpsertMenuItemRequest request)
    {
        var result = await _sender.Send(new UpsertMenuItemCommand(request.Id, menuId, request.Label, request.Url, request.Target, request.Icon, request.ParentId, request.SortOrder));
        return Ok(result);
    }

    [HttpDelete("items/{itemId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteItem(Guid itemId)
    {
        await _sender.Send(new DeleteMenuItemCommand(itemId));
        return NoContent();
    }
}

public record UpdateMenuRequest(string Name, string Location, bool IsActive);
public record UpsertMenuItemRequest(Guid? Id, string Label, string? Url, string? Target, string? Icon, Guid? ParentId, int SortOrder);
