using GiapTech.Ipages.Application.Carts.Commands;
using GiapTech.Ipages.Application.Carts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/carts")]
public class CartsController : ControllerBase
{
    private readonly ISender _sender;

    public CartsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? sessionId, [FromQuery] Guid? customerId)
    {
        var result = await _sender.Send(new GetCartQuery(sessionId, customerId));
        return result == null ? NoContent() : Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPatch("items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid itemId, [FromBody] UpdateCartItemRequest request)
    {
        await _sender.Send(new UpdateCartItemCommand(itemId, request.Quantity));
        return NoContent();
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId)
    {
        await _sender.Send(new RemoveCartItemCommand(itemId));
        return NoContent();
    }

    [HttpDelete("{cartId:guid}")]
    public async Task<IActionResult> Clear(Guid cartId)
    {
        await _sender.Send(new ClearCartCommand(cartId));
        return NoContent();
    }
}

public record UpdateCartItemRequest(int Quantity);
