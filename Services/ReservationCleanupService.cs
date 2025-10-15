
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class ReservationCleanupService : BackgroundService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReservationCleanupService> _logger;
    private readonly AuditService _audit;

    public ReservationCleanupService(AppDbContext context, ILogger<ReservationCleanupService> logger, AuditService audit)
    {
        _context = context;
        _logger = logger;
        _audit = audit;
    }

    public async Task ReleaseExpiredReservations()
    {
        var expiredTickets = await _context.Tickets
            .Where(t => t.status == "reserved" && t.reserved_until < DateTime.UtcNow)
            .ToListAsync();

        foreach (var ticket in expiredTickets)
        {
            ticket.status = "available";
            ticket.user_id = null;
            ticket.reserved_until = null;

            var spectacle = await _context.Spectacles.FindAsync(ticket.spectacle_id);
            if (spectacle != null)
                spectacle.seats_booked--;
        }

        await _context.SaveChangesAsync();
        if (expiredTickets.Count > 0)
        {
            await _audit.LogAction(
                null,
                "RELEASE_RESERVATION",
                "ticket",
                0,
                null,
                null,
                System.Text.Json.JsonSerializer.Serialize(new { action = "release_expired_reservations", count = expiredTickets.Count }));
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Проверка просроченных бронирований...");

            var expiredTickets = await _context.Tickets
                .Where(t => t.status == "reserved" && t.reserved_until < DateTime.UtcNow)
                .ToListAsync();

            foreach (var ticket in expiredTickets)
            {
                ticket.status = "available";
                ticket.user_id = null;
                ticket.reserved_until = null;
                
                var spectacle = await _context.Spectacles.FindAsync(ticket.spectacle_id);
                if (spectacle != null)
                    spectacle.seats_booked--;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Освобождено {expiredTickets.Count} просроченных бронирований.");

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}


