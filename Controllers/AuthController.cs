
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
            var user = await _authService.Register(model.email, model.password, model.first_name, model.last_name);
            return Ok(new { Message = "Пользователь успешно зарегистрирован." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            var token = await _authService.Login(model.email, model.password);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }
}

public class RegisterModel
{
    public string email { get; set; }
    public string password { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
}

public class LoginModel
{
    public string email { get; set; }
    public string password { get; set; }
}
