using black_follow.Data;

public class MyBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MyBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
            // Use dbContext for operations
        }
    }
}