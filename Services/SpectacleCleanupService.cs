
using Microsoft.EntityFrameworkCore;


public class SpectacleCleanupService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SpectacleCleanupService> _logger;
    private readonly AuditService _audit;

    public SpectacleCleanupService(AppDbContext context, ILogger<SpectacleCleanupService> logger, AuditService audit)
    {
        _context = context;
        _logger = logger;
        _audit = audit;
    }

    public async Task CleanupFinishedSpectacles()
    {
        await CreateUserVisitsForStartedSpectacles();

        // delete tickets for deleted spectacles
        var threshold = DateTime.UtcNow - TimeSpan.FromHours(1);
        var candidates = await _context.Spectacles
            .Where(s => s.start_time < threshold)
            .ToListAsync();

        var finishedSpectacles = candidates
            .Where(s => s.start_time.Add(s.duration) < DateTime.UtcNow)
            .ToList();

        foreach (var spectacle in finishedSpectacles)
        {
            var tickets = await _context.Tickets
                .Where(t => t.spectacle_id == spectacle.id)
                .ToListAsync();
            _context.Tickets.RemoveRange(tickets);

            _logger.LogInformation($"Удалены билеты для спектакля {spectacle.title}.");
            await _audit.LogAction(
                null,
                "DELETE_TICKETS",
                "ticket",
                spectacle.id,
                null,
                null,
                System.Text.Json.JsonSerializer.Serialize(new { action = "delete_tickets", spectacle_id = spectacle.id, count = tickets.Count }));
        }

        await _context.SaveChangesAsync();
    }

    private async Task CreateUserVisitsForStartedSpectacles()
    {
        var allSpectacles = await _context.Spectacles.ToListAsync();
        var startedSpectacles = allSpectacles
            .Where(s => s.start_time <= DateTime.UtcNow && s.start_time.Add(s.duration) > DateTime.UtcNow)
            .ToList();

        foreach (var spectacle in startedSpectacles)
        {
            var existingVisits = await _context.UserVisits
                .Where(uv => uv.spectacle_id == spectacle.id)
                .AnyAsync();

            if (!existingVisits)
            {
                var purchasedTickets = await _context.Tickets
                    .Where(t => t.spectacle_id == spectacle.id && t.status == "purchased")
                    .ToListAsync();

                foreach (var ticket in purchasedTickets)
                {
                    _context.UserVisits.Add(new UserVisit
                    {
                        user_id = ticket.user_id.Value,
                        spectacle_id = spectacle.id,
                        seat = ticket.seat,
                        price = ticket.price,
                        visit_date = spectacle.start_time
                    });
                }

                _logger.LogInformation($"Созданы UserVisit записи для спектакля {spectacle.title}.");
            }
        }
    }
}
