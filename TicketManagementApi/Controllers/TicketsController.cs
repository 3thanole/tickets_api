using Microsoft.AspNetCore.Mvc;
using TicketManagementApi.DTOs;
using TicketManagementApi.Services;

namespace TicketManagementApi.Controllers;

[ApiController]
[Route("tickets")]
public class TicketsController(ITicketService ticketService) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<TicketResponse>> GetAll() => Ok(ticketService.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<TicketResponse> GetById(int id)
    {
        var ticket = ticketService.GetById(id);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost]
    public ActionResult<TicketResponse> Create(CreateTicketRequest request)
    {
        var created = ticketService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<TicketResponse> Update(int id, UpdateTicketRequest request)
    {
        var updated = ticketService.Update(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPatch("{id:int}/status")]
    public ActionResult<TicketResponse> UpdateStatus(int id, UpdateTicketStatusRequest request)
    {
        var updated = ticketService.UpdateStatus(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) => ticketService.Delete(id) ? NoContent() : NotFound();

    [HttpPost("{id:int}/comments")]
    public ActionResult<TicketCommentResponse> AddComment(int id, AddCommentRequest request)
    {
        var comment = ticketService.AddComment(id, request);
        return comment is null ? NotFound() : CreatedAtAction(nameof(GetById), new { id }, comment);
    }
}
