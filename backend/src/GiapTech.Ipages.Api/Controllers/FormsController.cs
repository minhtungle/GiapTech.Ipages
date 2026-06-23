using GiapTech.Ipages.Application.Forms.Commands;
using GiapTech.Ipages.Application.Forms.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiapTech.Ipages.Api.Controllers;

[ApiController]
[Route("api/v1/forms")]
public class FormsController : ControllerBase
{
    private readonly ISender _sender;

    public FormsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new GetFormsQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{formId:guid}/entries")]
    [Authorize]
    public async Task<IActionResult> GetEntries(Guid formId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? isRead = null)
    {
        var result = await _sender.Send(new GetFormEntriesQuery(formId, page, pageSize, isRead));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateFormCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFormRequest request)
    {
        var result = await _sender.Send(new UpdateFormCommand(id, request.Name, request.Description, request.Fields, request.SuccessMessage, request.NotifyEmails, request.IsActive));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _sender.Send(new DeleteFormCommand(id));
        return NoContent();
    }

    [HttpPost("{formId:guid}/submit")]
    public async Task<IActionResult> Submit(Guid formId, [FromBody] SubmitFormRequest request)
    {
        var result = await _sender.Send(new SubmitFormEntryCommand(formId, request.Data));
        return Ok(new { result.Id, Message = "Gửi thành công." });
    }

    [HttpPatch("entries/{entryId:guid}/read")]
    [Authorize]
    public async Task<IActionResult> MarkRead(Guid entryId, [FromBody] MarkReadRequest request)
    {
        await _sender.Send(new MarkEntryReadCommand(entryId, request.IsRead));
        return NoContent();
    }
}

public record UpdateFormRequest(string Name, string? Description, string Fields, string? SuccessMessage, string? NotifyEmails, bool IsActive);
public record SubmitFormRequest(string Data);
public record MarkReadRequest(bool IsRead);
