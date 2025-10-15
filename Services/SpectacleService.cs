
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class SpectacleService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SpectacleService> _logger;
    private readonly AuditService _auditService;

    public SpectacleService(AppDbContext context, ILogger<SpectacleService> logger, AuditService auditService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
    }


    public async Task<Spectacle> CreateSpectacle(CreateSpectacleModel model, int managerId)
    {
        // Проверка блокировки менеджера
        var manager = await _context.Users.FindAsync(managerId);
        if (manager == null)
            throw new Exception("Менеджер не найден.");
        
        if (manager.is_blocked)
            throw new Exception("Менеджер заблокирован и не может создавать спектакли.");

        // Нормализуем время начала к UTC, если Kind не задан
        if (model.start_time.Kind == DateTimeKind.Unspecified)
            model.start_time = DateTime.SpecifyKind(model.start_time, DateTimeKind.Utc);

        // Нельзя создавать спектакль, если он начинается менее чем через 1 час
        if (model.start_time - DateTime.UtcNow < TimeSpan.FromHours(1))
            throw new Exception("Нельзя создавать спектакль ранее чем через час от текущего времени.");

        var endTime = model.start_time.Add(model.duration);

        var hall = await _context.Halls.FindAsync(model.hall_id);
        if (hall == null)
            throw new Exception("Зал не найден.");

        var allSpectacles = await _context.Spectacles
            .Where(s => s.hall_id == model.hall_id)
            .ToListAsync();

        // Проверка пересечений и часового интервала
        var conflictingSpectacles = allSpectacles
            .Where(s =>
            {
                var existingStart = s.start_time;
                var existingEnd = s.start_time.Add(s.duration);
                var newStart = model.start_time;
                var newEnd = endTime;

                // Проверка пересечений
                bool hasOverlap = (newStart < existingEnd && newEnd > existingStart);
                
                // Проверка часового интервала (1 час между спектаклями)
                bool tooCloseAfter = (newStart - existingEnd).TotalHours < 1 && newStart > existingEnd;
                bool tooCloseBefore = (existingStart - newEnd).TotalHours < 1 && existingStart > newEnd;

                return hasOverlap || tooCloseAfter || tooCloseBefore;
            })
            .ToList();

        if (conflictingSpectacles.Any())
            throw new Exception("Конфликт времени с другим спектаклем в этом зале. Между спектаклями должен быть интервал не менее 1 часа.");

        // Проверяем рабочие часы по локальному времени пользователя/сервера
        var localStart = model.start_time.ToLocalTime();
        var localEnd = localStart.Add(model.duration);
        var startTime = localStart.TimeOfDay;
        var endTimeSpan = localEnd.TimeOfDay;
        var open = new TimeSpan(9, 0, 0);
        var close = new TimeSpan(23, 0, 0);
        if (startTime < open || startTime > close || endTimeSpan > close)
            throw new Exception("Спектакль должен проходить в рабочие часы зала (9:00–23:00).");

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            var spectacle = new Spectacle
            {
                title = model.title,
                description = model.description,
                start_time = model.start_time,
                duration = model.duration,
                price = model.price,
                hall_id = model.hall_id,
                seats_booked = 0,
                seats_bought = 0,
                money_earned = 0
            };

            _context.Spectacles.Add(spectacle);
            await _context.SaveChangesAsync();

            for (int seat = 1; seat <= hall.capacity; seat++)
            {
                _context.Tickets.Add(new Ticket
                {
                    spectacle_id = spectacle.id,
                    seat = seat,
                    status = "available",
                    price = model.price
                });
            }

            await _context.SaveChangesAsync();

            // Логирование создания спектакля
            await _auditService.LogAction(
                managerId,
                "CREATE",
                "spectacle",
                spectacle.id,
                null,
                new { spectacle.title, spectacle.start_time, spectacle.hall_id });

            // Логирование создания билетов
            await _auditService.LogAction(
                null,
                "CREATE_TICKETS",
                "ticket",
                spectacle.id,
                null,
                null,
                JsonSerializer.Serialize(new { action = "create_tickets", count = hall.capacity, spectacle_id = spectacle.id }));

            await transaction.CommitAsync();
            return spectacle;
        }
    }

    public async Task<Spectacle> UpdateSpectacle(int id, UpdateSpectacleModel model, int managerId)
    {
        // Проверка блокировки менеджера
        var manager = await _context.Users.FindAsync(managerId);
        if (manager == null)
            throw new Exception("Менеджер не найден.");
        
        if (manager.is_blocked)
            throw new Exception("Менеджер заблокирован и не может обновлять спектакли.");

        var spectacle = await _context.Spectacles.FindAsync(id);
        if (spectacle == null)
            throw new Exception("Спектакль не найден.");

        // Проверка времени (запрет изменений за 30 минут до начала)
        if (spectacle.start_time - DateTime.UtcNow < TimeSpan.FromMinutes(30))
            throw new Exception("Нельзя изменять спектакль за 30 минут до начала.");

        // Нормализуем время начала к UTC, если Kind не задан
        if (model.start_time.Kind == DateTimeKind.Unspecified)
            model.start_time = DateTime.SpecifyKind(model.start_time, DateTimeKind.Utc);

        // Вычисляем время окончания спектакля
        var endTime = model.start_time.Add(model.duration);

        var allSpectacles = await _context.Spectacles
            .Where(s => s.id != id && s.hall_id == model.hall_id)
            .ToListAsync();

        // Проверка пересечений и часового интервала
        var conflictingSpectacles = allSpectacles
            .Where(s =>
            {
                var existingStart = s.start_time;
                var existingEnd = s.start_time.Add(s.duration);
                var newStart = model.start_time;
                var newEnd = endTime;

                // Проверка пересечений
                bool hasOverlap = (newStart < existingEnd && newEnd > existingStart);
                
                // Проверка часового интервала (1 час между спектаклями)
                bool tooCloseAfter = (newStart - existingEnd).TotalHours < 1 && newStart > existingEnd;
                bool tooCloseBefore = (existingStart - newEnd).TotalHours < 1 && existingStart > newEnd;

                return hasOverlap || tooCloseAfter || tooCloseBefore;
            })
            .ToList();

        if (conflictingSpectacles.Any())
            throw new Exception("Конфликт времени с другим спектаклем в этом зале. Между спектаклями должен быть интервал не менее 1 часа.");

        // Проверяем рабочие часы по локальному времени
        var localStart = model.start_time.ToLocalTime();
        var localEnd = localStart.Add(model.duration);
        var startTime = localStart.TimeOfDay;
        var endTimeSpan = localEnd.TimeOfDay;
        var open = new TimeSpan(9, 0, 0);
        var close = new TimeSpan(23, 0, 0);
        if (startTime < open || startTime > close || endTimeSpan > close)
            throw new Exception("Спектакль должен проходить в рабочие часы зала (9:00–23:00).");

        // Обновление данных спектакля
        spectacle.title = model.title;
        spectacle.description = model.description;
        spectacle.start_time = model.start_time;
        spectacle.duration = model.duration;
        spectacle.price = model.price;

        // Если зал изменился, обновляем билеты
        if (spectacle.hall_id != model.hall_id)
        {
            var oldHall = await _context.Halls.FindAsync(spectacle.hall_id);
            var newHall = await _context.Halls.FindAsync(model.hall_id);

            // Получаем все существующие билеты
            var existingTickets = await _context.Tickets
                .Where(t => t.spectacle_id == spectacle.id)
                .ToListAsync();

            // Сохраняем занятые места, которые помещаются в новый зал
            var validTickets = existingTickets
                .Where(t => t.seat <= newHall.capacity && (t.status == "reserved" || t.status == "purchased"))
                .ToList();

            // Удаляем все старые билеты
            _context.Tickets.RemoveRange(existingTickets);

            // Создаём новые билеты
            for (int seat = 1; seat <= newHall.capacity; seat++)
            {
                // Проверяем, есть ли сохраненный билет для этого места
                var savedTicket = validTickets.FirstOrDefault(t => t.seat == seat);
                
                if (savedTicket != null)
                {
                    // Восстанавливаем сохраненный билет
                    _context.Tickets.Add(new Ticket
                    {
                        spectacle_id = spectacle.id,
                        seat = seat,
                        status = savedTicket.status,
                        user_id = savedTicket.user_id,
                        reserved_until = savedTicket.reserved_until,
                        price = model.price
                    });
                }
                else
                {
                    // Создаём новый свободный билет
                    _context.Tickets.Add(new Ticket
                    {
                        spectacle_id = spectacle.id,
                        seat = seat,
                        status = "available",
                        price = model.price
                    });
                }
            }

            // Логируем удаленные билеты (места, которые не поместились в новый зал)
            var removedTickets = existingTickets
                .Where(t => t.seat > newHall.capacity && (t.status == "reserved" || t.status == "purchased"))
                .ToList();

            if (removedTickets.Any())
            {
                // Обновляем статистику - уменьшаем количество забронированных и купленных мест
                var removedReserved = removedTickets.Count(t => t.status == "reserved");
                var removedPurchased = removedTickets.Count(t => t.status == "purchased");
                
                spectacle.seats_booked = Math.Max(0, spectacle.seats_booked - removedReserved);
                spectacle.seats_bought = Math.Max(0, spectacle.seats_bought - removedPurchased);
                
                // Возвращаем деньги за удаленные купленные билеты
                var refundAmount = removedTickets
                    .Where(t => t.status == "purchased")
                    .Sum(t => t.price);
                spectacle.money_earned = Math.Max(0, spectacle.money_earned - refundAmount);

                _logger.LogWarning($"При смене зала удалены билеты на места {string.Join(", ", removedTickets.Select(t => t.seat))} - не помещаются в новый зал (вместимость: {newHall.capacity}). Возвращено {refundAmount} за удаленные покупки.");
            }

            spectacle.hall_id = model.hall_id;
        }

        await _context.SaveChangesAsync();

        await _auditService.LogAction(
            managerId,
            "UPDATE",
            "spectacle",
            spectacle.id,
            null,
            new { spectacle.title, spectacle.start_time, spectacle.hall_id, spectacle.price });
        return spectacle;
    }



    public async Task DeleteSpectacle(int id, int managerId)
    {
        // Проверка блокировки менеджера
        var manager = await _context.Users.FindAsync(managerId);
        if (manager == null)
            throw new Exception("Менеджер не найден.");
        
        if (manager.is_blocked)
            throw new Exception("Менеджер заблокирован и не может удалять спектакли.");

        var spectacle = await _context.Spectacles.FindAsync(id);
        if (spectacle == null)
            throw new Exception("Спектакль не найден.");

        // Проверка времени (запрет удаления за 30 минут до начала)
        if (spectacle.start_time - DateTime.UtcNow < TimeSpan.FromMinutes(30))
            throw new Exception("Нельзя удалять спектакль за 30 минут до начала.");

        // Удаление билетов
        var tickets = await _context.Tickets
            .Where(t => t.spectacle_id == id)
            .ToListAsync();
        _context.Tickets.RemoveRange(tickets);

        // Удаление спектакля
        _context.Spectacles.Remove(spectacle);
        await _context.SaveChangesAsync();

        // Логирование удаления спектакля и билетов (как системное действие)
        await _auditService.LogAction(managerId, "DELETE", "spectacle", spectacle.id, new { spectacle.title }, null);
        await _auditService.LogAction(null, "DELETE_TICKETS", "ticket", spectacle.id, null, null, System.Text.Json.JsonSerializer.Serialize(new { action = "delete_tickets_with_spectacle", spectacle_id = spectacle.id, count = tickets.Count }));
    }

    public async Task<SpectacleStatistics> GetSpectacleStatistics(int spectacleId)
    {
        var spectacle = await _context.Spectacles
            .Include(s => s.hall)
            .FirstOrDefaultAsync(s => s.id == spectacleId);

        if (spectacle == null)
            throw new Exception("Спектакль не найден.");

        var totalSeats = spectacle.hall.capacity;
        var availableSeats = totalSeats - spectacle.seats_booked - spectacle.seats_bought;

        return new SpectacleStatistics
        {
            SpectacleId = spectacle.id,
            Title = spectacle.title,
            TotalSeats = totalSeats,
            AvailableSeats = availableSeats,
            BookedSeats = spectacle.seats_booked,
            BoughtSeats = spectacle.seats_bought,
            MoneyEarned = spectacle.money_earned,
            StartTime = spectacle.start_time,
            Duration = spectacle.duration
        };
    }

    public async Task<List<SpectaclePublicDto>> GetSpectaclesByHallId(int hallId)
    {
        return await _context.Spectacles
            .Include(s => s.hall)
            .Where(s => s.hall_id == hallId)
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
    }


}
