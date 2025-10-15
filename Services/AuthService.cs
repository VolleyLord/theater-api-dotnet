
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly AuditService _audit;

    public AuthService(AppDbContext context, IConfiguration configuration, AuditService audit)
    {
        _context = context;
        _configuration = configuration;
        _audit = audit;
    }

    public async Task<User> Register(string email, string password, string first_name, string last_name)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        if (existingUser != null)
            throw new Exception("Пользователь с таким email уже существует.");

        var user = new User
        {
            email = email,
            password_hash = BCrypt.Net.BCrypt.HashPassword(password),
            first_name = first_name,
            last_name = last_name,
            role_id = 3, // "user" role default
            is_blocked = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        await _audit.LogAction(user.id, "REGISTER", "user", user.id, null, new { user.email });
        return user;
    }

    public async Task<string> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.password_hash))
            throw new Exception("Неверный email или пароль.");

        return await GenerateJwtTokenAsync(user);
    }

    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.id == user.role_id);
        if (role == null)
        {
            throw new InvalidOperationException("User role not found");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
        new Claim(ClaimTypes.Role, role.name),
        new Claim(ClaimTypes.Email, user.email)
    };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}
