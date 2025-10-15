
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ReservationCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ReservationCleanupWorker> _logger;

    public ReservationCleanupWorker(IServiceProvider services, ILogger<ReservationCleanupWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Проверка просроченных бронирований...");

            using (var scope = _services.CreateScope())
            {
                var cleanupService = scope.ServiceProvider.GetRequiredService<ReservationCleanupService>();
                await cleanupService.ReleaseExpiredReservations();
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
