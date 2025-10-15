
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/tickets")]
[Authorize(Roles = "user")] 
public class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    
    [HttpPost("buy")]
    public async Task<IActionResult> BuyTicket([FromBody] BuyTicketModel model)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var ticket = await _ticketService.BuyTicket(model.spectacle_id, model.seat, userId, model.ticket_id);
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpPost("book")]
    public async Task<IActionResult> BookTicket([FromBody] BookTicketModel model)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var ticket = await _ticketService.BookTicket(model.spectacle_id, model.seat, userId);
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}


public class BuyTicketModel
{
    public int spectacle_id { get; set; }
    public int seat { get; set; }
    public int? ticket_id { get; set; } // buy booked ticket
}

public class BookTicketModel
{
    public int spectacle_id { get; set; }
    public int seat { get; set; }
}
