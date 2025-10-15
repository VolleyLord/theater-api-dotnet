using Microsoft.EntityFrameworkCore;

public class HallService
{
    private readonly AppDbContext _context;
    private readonly ILogger<HallService> _logger;

    public HallService(AppDbContext context, ILogger<HallService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Hall> CreateHall(CreateHallModel model, int managerId)
    {
        var manager = await _context.Users.FindAsync(managerId);
        if (manager == null)
            throw new Exception("Менеджер не найден.");
        
        if (manager.is_blocked)
            throw new Exception("Менеджер заблокирован и не может создавать залы.");

        var hall = new Hall
        {
            name = model.name,
            capacity = model.capacity
        };

        _context.Halls.Add(hall);
        await _context.SaveChangesAsync();

        return hall;
    }

    public async Task<Hall> UpdateHall(int id, UpdateHallModel model, int managerId)
    {
        var manager = await _context.Users.FindAsync(managerId);
        if (manager == null)
            throw new Exception("Менеджер не найден.");
        
        if (manager.is_blocked)
            throw new Exception("Менеджер заблокирован и не может обновлять залы.");

        var hall = await _context.Halls.FindAsync(id);
        if (hall == null)
            throw new Exception("Зал не найден.");

        hall.name = model.name;
        hall.capacity = model.capacity;

        await _context.SaveChangesAsync();

        return hall;
    }

    public async Task DeleteHall(int id, int managerId)
    {
        var manager = await _context.Users.FindAsync(managerId);
        if (manager == null)
            throw new Exception("Менеджер не найден.");
        
        if (manager.is_blocked)
            throw new Exception("Менеджер заблокирован и не может удалять залы.");

        var hall = await _context.Halls.FindAsync(id);
        if (hall == null)
            throw new Exception("Зал не найден.");

        var spectaclesInHall = await _context.Spectacles
            .Where(s => s.hall_id == id)
            .ToListAsync();

        if (spectaclesInHall.Any())
        {
            foreach (var spectacle in spectaclesInHall)
            {
                spectacle.hall_id = null;
            }
        }

        _context.Halls.Remove(hall);
        await _context.SaveChangesAsync();
    }
}
