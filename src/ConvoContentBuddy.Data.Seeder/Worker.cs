namespace ConvoContentBuddy.Data.Seeder;

/// <summary>
/// Background service for seeding data into the knowledge base.
/// </summary>
/// <param name="logger">The logger instance.</param>
public class Worker(ILogger<Worker> logger) : BackgroundService
{
    /// <summary>
    /// Executes the background seeding process.
    /// </summary>
    /// <param name="stoppingToken">Token to signal cancellation.</param>
    /// <returns>A task representing the async operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
