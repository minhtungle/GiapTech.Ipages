using GiapTech.Ipages.Application.Orders.Commands;
using GiapTech.Ipages.Application.Orders.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] OrderStatus? status = null)
    {
        var result = await _sender.Send(new GetOrdersQuery(page, pageSize, search, status));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetOrderByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        await _sender.Send(new UpdateOrderStatusCommand(id, request.Status, request.CancelReason));
        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest request)
    {
        await _sender.Send(new CancelOrderCommand(id, request.Reason));
        return NoContent();
    }
}

public record UpdateOrderStatusRequest(OrderStatus Status, string? CancelReason);
public record CancelOrderRequest(string? Reason);
