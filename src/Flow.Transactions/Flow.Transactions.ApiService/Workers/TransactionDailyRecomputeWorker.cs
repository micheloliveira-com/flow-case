using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;
using Flow.Transactions.Infrastructure.Messaging.Consumers;

public sealed class TransactionDailyRecomputeWorker(
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var service = scope.ServiceProvider
            .GetRequiredService<ITransactionDailyRecomputeConsumer>();

        await service.StartAsync(
            async message =>
            {
                using var innerScope = scopeFactory.CreateScope();

                var useCase = innerScope.ServiceProvider
                    .GetRequiredService<IExecuteTransactionDailyRecomputeService>();

                await useCase.ExecuteAsync(message);
            },
            stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}