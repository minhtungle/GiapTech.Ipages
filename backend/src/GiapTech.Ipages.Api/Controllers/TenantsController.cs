using GiapTech.Ipages.Application.Common.Exceptions;
using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Tenants.Commands;
using GiapTech.Ipages.Application.Tenants.Queries;
using GiapTech.Ipages.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentTenantService _tenantService;

    public TenantsController(ISender sender, ICurrentTenantService tenantService)
    {
        _sender = sender;
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] TenantStatus? status = null)
    {
        if (!_tenantService.IsHostAdmin)
            throw new ForbiddenException();

        var result = await _sender.Send(new GetTenantsQuery(page, pageSize, search, status));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!_tenantService.IsHostAdmin)
            throw new ForbiddenException();

        var result = await _sender.Send(new GetTenantByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand command)
    {
        if (!_tenantService.IsHostAdmin)
            throw new ForbiddenException();

        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request)
    {
        if (!_tenantService.IsHostAdmin)
            throw new ForbiddenException();

        var result = await _sender.Send(new UpdateTenantCommand(id, request.Name, request.Email, request.Phone, request.Address, request.Description, request.Status, request.ExpiresAt, request.AdminUsername, request.AdminPassword));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!_tenantService.IsHostAdmin)
            throw new ForbiddenException();

        await _sender.Send(new DeleteTenantCommand(id));
        return NoContent();
    }
}

public record UpdateTenantRequest(string Name, string? Email, string? Phone, string? Address, string? Description, TenantStatus Status, DateTime? ExpiresAt, string? AdminUsername, string? AdminPassword);
