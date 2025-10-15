public class UserUnblockWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<UserUnblockWorker> _logger;

    public UserUnblockWorker(IServiceProvider services, ILogger<UserUnblockWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Проверка пользователей на разблокировку...");

            using (var scope = _services.CreateScope())
            {
                var unblockService = scope.ServiceProvider.GetRequiredService<UserUnblockService>();
                await unblockService.UnblockExpiredUsers();
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
