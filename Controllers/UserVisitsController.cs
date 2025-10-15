using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/users/me")]
[Authorize(Roles = "user")] 
public class UserVisitsController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserVisitsController(AppDbContext context)
    {
        _context = context;
    }

    
    [HttpGet("visits")]
    public async Task<IActionResult> GetUserVisits()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var visits = await _context.UserVisits
            .Include(uv => uv.spectacle)
            .Where(uv => uv.user_id == userId)
            .OrderByDescending(uv => uv.visit_date)
            .ToListAsync();

        return Ok(visits);
    }
}
