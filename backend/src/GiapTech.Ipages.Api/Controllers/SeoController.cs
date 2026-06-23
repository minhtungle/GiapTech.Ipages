using GiapTech.Ipages.Application.Seo.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/seo")]
public class SeoController : ControllerBase
{
    private readonly ISender _sender;

    public SeoController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string entityType, [FromQuery] Guid entityId)
    {
        var result = await _sender.Send(new GetSeoMetadataQuery(entityType, entityId));
        return result == null ? NoContent() : Ok(result);
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Upsert([FromBody] UpsertSeoMetadataCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }
}
