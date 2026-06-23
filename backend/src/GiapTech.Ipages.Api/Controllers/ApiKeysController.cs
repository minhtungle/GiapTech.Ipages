using GiapTech.Ipages.Application.ApiKeys.Commands;
using GiapTech.Ipages.Application.ApiKeys.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/api-keys")]
[Authorize]
public class ApiKeysController : ControllerBase
{
    private readonly ISender _sender;

    public ApiKeysController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new GetApiKeysQuery(page, pageSize));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] GenerateApiKeyCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        await _sender.Send(new RevokeApiKeyCommand(id));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteApiKeyCommand(id));
        return NoContent();
    }
}
