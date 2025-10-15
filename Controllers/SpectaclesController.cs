
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/spectacles")]
public class SpectaclesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SpectacleService _spectacleService;

    public SpectaclesController(SpectacleService spectacleService, AppDbContext context)
    {
        _spectacleService = spectacleService;
        _context = context;
    }

    
    [AllowAnonymous] // Доступно для всех, включая Guest
    [HttpGet]
    public async Task<IActionResult> GetAllSpectacles()
    {
        var spectacles = await _context.Spectacles
            .Include(s => s.hall)
            .Where(s => s.start_time > DateTime.UtcNow) 
            .Select(s => new SpectaclePublicDto
            {
                id = s.id,
                title = s.title,
                description = s.description,
                start_time = s.start_time,
                duration = s.duration,
                price = s.price,
                hall_id = s.hall_id,
                hall = s.hall
            })
            .ToListAsync();

        return Ok(spectacles);
    }

    
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpectacle(int id)
    {
        var spectacle = await _context.Spectacles
            .Include(s => s.hall)
            .FirstOrDefaultAsync(s => s.id == id);

        if (spectacle == null)
            return NotFound();

        var spectacleDto = new SpectaclePublicDto
        {
            id = spectacle.id,
            title = spectacle.title,
            description = spectacle.description,
            start_time = spectacle.start_time,
            duration = spectacle.duration,
            price = spectacle.price,
            hall_id = spectacle.hall_id,
            hall = spectacle.hall
        };

        return Ok(spectacleDto);
    }


    
    [AllowAnonymous]
    [HttpGet("{id}/available-seats")]
    public async Task<IActionResult> GetAvailableSeats(int id)
    {
        var spectacle = await _context.Spectacles.FindAsync(id);
        if (spectacle == null)
            return NotFound();

        var availableSeats = await _context.Tickets
            .Where(t => t.spectacle_id == id && t.status == "available")
            .Select(t => t.seat)
            .ToListAsync();

        return Ok(new { spectacle_id = id, available_seats = availableSeats });
    }


    [Authorize(Roles = "manager")]
    [HttpPost]
    public async Task<IActionResult> CreateSpectacle([FromBody] CreateSpectacleModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var spectacle = await _spectacleService.CreateSpectacle(model, userId);
            return Ok(spectacle);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }


    [Authorize(Roles = "manager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSpectacle(int id, [FromBody] UpdateSpectacleModel model)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var spectacle = await _spectacleService.UpdateSpectacle(id, model, userId);
            return Ok(spectacle);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }


    [Authorize(Roles = "manager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSpectacle(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _spectacleService.DeleteSpectacle(id, userId);
            return Ok(new { Message = "Спектакль успешно удалён." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [Authorize(Roles = "manager")]
    [HttpGet("{id}/statistics")]
    public async Task<IActionResult> GetSpectacleStatistics(int id)
    {
        try
        {
            var statistics = await _spectacleService.GetSpectacleStatistics(id);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // GET: api/spectacles/by-hall/{hallId}
    [Authorize(Roles = "user,manager")]
    [HttpGet("by-hall/{hallId}")]
    public async Task<IActionResult> GetSpectaclesByHallId(int hallId)
    {
        var spectacles = await _spectacleService.GetSpectaclesByHallId(hallId);
        return Ok(spectacles);
    }


}
