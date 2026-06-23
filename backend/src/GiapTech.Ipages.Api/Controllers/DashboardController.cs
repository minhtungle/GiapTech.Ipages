using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Application.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentTenantService _tenantService;

    public DashboardController(ISender sender, ICurrentTenantService tenantService)
    {
        _sender = sender;
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (_tenantService.IsHostAdmin)
        {
            var result = await _sender.Send(new GetHostDashboardQuery());
            return Ok(result);
        }
        else
        {
            var result = await _sender.Send(new GetTenantDashboardQuery());
            return Ok(result);
        }
    }
}
