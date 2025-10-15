
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _adminService.GetUserById(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsers();
        return Ok(users);
    }

    
    [HttpGet("users/by-role/{roleId}")]
    public async Task<IActionResult> GetUsersByRole(int roleId)
    {
        var users = await _adminService.GetUsersByRole(roleId);
        return Ok(users);
    }

    
    [HttpGet("users/blocked")]
    public async Task<IActionResult> GetBlockedUsers()
    {
        var users = await _adminService.GetBlockedUsers();
        return Ok(users);
    }

    
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _adminService.CreateUser(adminId, model);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _adminService.UpdateUser(adminId, id, model);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpPut("users/{id}/password")]
    public async Task<IActionResult> UpdateUserPassword(int id, [FromBody] UpdateUserPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _adminService.UpdateUserPassword(adminId, id, model);
            return Ok(new { Message = "Пароль успешно обновлён." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _adminService.DeleteUser(adminId, id);
            return Ok(new { Message = "Пользователь успешно удалён." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpPost("users/{id}/block")]
    public async Task<IActionResult> BlockUser(int id, [FromBody] BlockUserModel model)
    {
        try
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _adminService.BlockUser(adminId, id, model.blocked_until);
            return Ok(new { Message = "Пользователь успешно заблокирован." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpPost("users/{id}/unblock")]
    public async Task<IActionResult> UnblockUser(int id)
    {
        try
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _adminService.UnblockUser(adminId, id);
            return Ok(new { Message = "Пользователь успешно разблокирован." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    
    [HttpGet("action-history")]
    public async Task<IActionResult> GetActionHistory()
    {
        var actionHistory = await _adminService.GetActionHistory();
        return Ok(actionHistory);
    }

}
