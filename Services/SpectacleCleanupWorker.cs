
public class SpectacleCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SpectacleCleanupWorker> _logger;

    public SpectacleCleanupWorker(IServiceProvider services, ILogger<SpectacleCleanupWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Проверка завершённых спектаклей...");

            using (var scope = _services.CreateScope())
            {
                var cleanupService = scope.ServiceProvider.GetRequiredService<SpectacleCleanupService>();
                await cleanupService.CleanupFinishedSpectacles();
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
