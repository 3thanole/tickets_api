using Microsoft.AspNetCore.Mvc;
using TicketManagementApi.DTOs;
using TicketManagementApi.Services;

namespace TicketManagementApi.Controllers;

[ApiController]
[Route("tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public ActionResult<List<TicketResponse>> GetAll() => Ok(_ticketService.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<TicketResponse> GetById(int id)
    {
        var ticket = _ticketService.GetById(id);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost]
    public ActionResult<TicketResponse> Create(CreateTicketRequest request)
    {
        var created = _ticketService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<TicketResponse> Update(int id, UpdateTicketRequest request)
    {
        var updated = _ticketService.Update(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPatch("{id:int}/status")]
    public ActionResult<TicketResponse> UpdateStatus(int id, UpdateTicketStatusRequest request)
    {
        var updated = _ticketService.UpdateStatus(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) => _ticketService.Delete(id) ? NoContent() : NotFound();
}
