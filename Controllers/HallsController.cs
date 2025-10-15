
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/halls")]
[Authorize(Roles = "manager")]
public class HallsController : ControllerBase
{
    private readonly HallService _hallService;
    private readonly AppDbContext _context; 

    public HallsController(HallService hallService, AppDbContext context) 
    {
        _hallService = hallService;
        _context = context; 
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateHall([FromBody] CreateHallModel model)
    {
        try
        {
            var managerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var hall = await _hallService.CreateHall(model, managerId);
            return Ok(hall);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHall(int id, [FromBody] UpdateHallModel model)
    {
        try
        {
            var managerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var hall = await _hallService.UpdateHall(id, model, managerId);
            return Ok(hall);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHall(int id)
    {
        try
        {
            var managerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            await _hallService.DeleteHall(id, managerId);
            return Ok(new { Message = "Зал успешно удалён." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpGet]
    public async Task<IActionResult> GetAllHalls()
    {
        var halls = await _context.Halls.ToListAsync();
        return Ok(halls);
    }
}
