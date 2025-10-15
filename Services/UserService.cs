
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly AuditService _audit;

    public UserService(AppDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<User> UpdateUser(int userId, UpdateUserSelfModel model)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Пользователь не найден.");

        user.email = model.email;
        user.first_name = model.first_name;
        user.last_name = model.last_name;

        await _context.SaveChangesAsync();
        await _audit.LogAction(userId, "UPDATE_SELF", "user", user.id, null, new { user.email, user.first_name, user.last_name });

        return user;
    }

    public async Task UpdateUserPassword(int userId, UpdateUserPasswordModel model)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Пользователь не найден.");

        user.password_hash = BCrypt.Net.BCrypt.HashPassword(model.password);

        await _context.SaveChangesAsync();
        await _audit.LogAction(userId, "UPDATE_PASSWORD_SELF", "user", user.id, null, null);
    }

    public async Task DeleteUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Пользователь не найден.");

        var userVisits = await _context.UserVisits.Where(uv => uv.user_id == userId).ToListAsync();
        _context.UserVisits.RemoveRange(userVisits);

        var reservedTickets = await _context.Tickets.Where(t => t.user_id == userId && t.status == "reserved").ToListAsync();
        foreach (var ticket in reservedTickets)
        {
            ticket.user_id = null;
            ticket.status = "available";
            ticket.reserved_until = null;
        }

        var actionHistories = await _context.ActionHistory.Where(ah => ah.user_id == userId).ToListAsync();
        foreach (var actionHistory in actionHistories)
        {
            actionHistory.user_id = null;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        await _audit.LogAction(userId, "DELETE_SELF", "user", userId, new { user.email }, null);
    }

    public async Task<List<Ticket>> GetMyActiveReservations(int userId)
    {
        return await _context.Tickets
            .Include(t => t.spectacle)
            .ThenInclude(s => s.hall)
            .Where(t => t.user_id == userId && t.status == "reserved" && t.reserved_until > DateTime.UtcNow)
            .ToListAsync();
    }

}
