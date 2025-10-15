
using Microsoft.EntityFrameworkCore;


public class AdminService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminService> _logger;
    private readonly AuditService _audit;

    public AdminService(AppDbContext context, ILogger<AdminService> logger, AuditService audit)
    {
        _context = context;
        _logger = logger;
        _audit = audit;
    }

    public async Task<User> GetUserById(int userId)
    {
        return await _context.Users
            .Include(u => u.role)
            .FirstOrDefaultAsync(u => u.id == userId);
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await _context.Users
            .Include(u => u.role)
            .ToListAsync();
    }

    public async Task<List<User>> GetUsersByRole(int roleId)
    {
        return await _context.Users
            .Include(u => u.role)
            .Where(u => u.role_id == roleId)
            .ToListAsync();
    }

    public async Task<List<User>> GetBlockedUsers()
    {
        return await _context.Users
            .Include(u => u.role)
            .Where(u => u.is_blocked)
            .ToListAsync();
    }

    public async Task<User> CreateUser(int adminId, CreateUserModel model)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == model.email);
        if (existingUser != null)
            throw new Exception("Пользователь с таким email уже существует.");

        var user = new User
        {
            email = model.email,
            password_hash = BCrypt.Net.BCrypt.HashPassword(model.password),
            first_name = model.first_name,
            last_name = model.last_name,
            role_id = model.role_id,
            is_blocked = false,
            blocked_until = null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _audit.LogAction(adminId, "CREATE", "user", user.id, null, new { user.email, user.first_name, user.last_name, user.role_id });

        return user;
    }

    public async Task<User> UpdateUser(int adminId, int userId, UpdateUserModel model)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Пользователь не найден.");

        user.email = model.email;
        user.first_name = model.first_name;
        user.last_name = model.last_name;
        user.role_id = model.role_id;

        await _context.SaveChangesAsync();

        await _audit.LogAction(adminId, "UPDATE", "user", user.id, null, new { user.email, user.first_name, user.last_name, user.role_id });

        return user;
    }

    public async Task UpdateUserPassword(int adminId, int userId, UpdateUserPasswordModel model)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Пользователь не найден.");

        user.password_hash = BCrypt.Net.BCrypt.HashPassword(model.password);

        await _context.SaveChangesAsync();
        await _audit.LogAction(adminId, "UPDATE_PASSWORD", "user", user.id, null, null);
    }

    public async Task DeleteUser(int adminId, int userId)
    {
        var user = await _context.Users
            .Include(u => u.role)
            .FirstOrDefaultAsync(u => u.id == userId);
        
        if (user == null)
            throw new Exception("Пользователь не найден.");

        if (user.role.name == "admin")
            throw new Exception("Нельзя удалять администраторов.");

        var userVisits = await _context.UserVisits.Where(uv => uv.user_id == userId).ToListAsync();
        _context.UserVisits.RemoveRange(userVisits);

        var actionHistories = await _context.ActionHistory.Where(ah => ah.user_id == userId).ToListAsync();
        foreach (var actionHistory in actionHistories)
        {
            actionHistory.user_id = null;
        }

        var tickets = await _context.Tickets.Where(t => t.user_id == userId).ToListAsync();
        foreach (var ticket in tickets)
        {
            ticket.user_id = null;
            ticket.status = "available";
            ticket.reserved_until = null;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        await _audit.LogAction(adminId, "DELETE", "user", userId, new { user.email }, null);
    }

    public async Task BlockUser(int adminId, int userId, DateTime? blockedUntil)
    {
        var user = await _context.Users
            .Include(u => u.role)
            .FirstOrDefaultAsync(u => u.id == userId);
        
        if (user == null)
            throw new Exception("Пользователь не найден.");

        if (user.role.name == "admin")
            throw new Exception("Нельзя блокировать администраторов.");

        user.is_blocked = true;
        user.blocked_until = blockedUntil;

        await _context.SaveChangesAsync();
        await _audit.LogAction(adminId, "BLOCK", "user", user.id, null, new { blocked_until = user.blocked_until });
    }

    public async Task UnblockUser(int adminId, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Пользователь не найден.");

        user.is_blocked = false;
        user.blocked_until = null;

        await _context.SaveChangesAsync();
        await _audit.LogAction(adminId, "UNBLOCK", "user", user.id, null, null);
    }

    public async Task<List<ActionHistory>> GetActionHistory()
    {
        return await _context.ActionHistory
            .OrderByDescending(ah => ah.timestamp)
            .ToListAsync();
    }

}
