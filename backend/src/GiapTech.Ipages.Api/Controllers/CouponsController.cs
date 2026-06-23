using GiapTech.Ipages.Application.Coupons.Commands;
using GiapTech.Ipages.Application.Coupons.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/coupons")]
public class CouponsController : ControllerBase
{
    private readonly ISender _sender;

    public CouponsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] bool? isActive = null)
    {
        var result = await _sender.Send(new GetCouponsQuery(page, pageSize, search, isActive));
        return Ok(result);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateCouponRequest request)
    {
        var result = await _sender.Send(new ValidateCouponCommand(request.Code, request.OrderAmount));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCouponCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponRequest request)
    {
        var result = await _sender.Send(new UpdateCouponCommand(id, request.Name, request.Description, request.Value, request.MinOrderAmount, request.MaxDiscount, request.UsageLimit, request.StartsAt, request.ExpiresAt, request.IsActive));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteCouponCommand(id));
        return NoContent();
    }
}

public record ValidateCouponRequest(string Code, decimal OrderAmount);
public record UpdateCouponRequest(string? Name, string? Description, decimal Value, decimal? MinOrderAmount, decimal? MaxDiscount, int? UsageLimit, DateTime? StartsAt, DateTime? ExpiresAt, bool IsActive);
