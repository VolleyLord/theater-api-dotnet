
using Microsoft.EntityFrameworkCore;

public class UserUnblockService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserUnblockService> _logger;

    public UserUnblockService(AppDbContext context, ILogger<UserUnblockService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task UnblockExpiredUsers()
    {
        var now = DateTime.UtcNow;
        var usersToUnblock = await _context.Users
            .Where(u => u.is_blocked && u.blocked_until <= now)
            .ToListAsync();

        foreach (var user in usersToUnblock)
        {
            user.is_blocked = false;
            user.blocked_until = null;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Разблокировано {usersToUnblock.Count} пользователей.");
    }
}
