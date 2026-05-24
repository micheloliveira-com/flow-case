using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;

public sealed class TransactionDailyRecomputeWorker(
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IExecuteTransactionDailyRecomputeService>();

        await service.ExecuteAsync(stoppingToken);


        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}