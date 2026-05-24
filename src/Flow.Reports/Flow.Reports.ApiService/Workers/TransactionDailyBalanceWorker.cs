using Flow.Reports.Application.UseCases.DailyBalance.ExecuteTransactionDailyBalance;
using Flow.Transactions.Infrastructure.Messaging.Consumers;

public sealed class TransactionDailyBalanceWorker(
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var consumer = scope.ServiceProvider
            .GetRequiredService<ITransactionDailyBalanceConsumer>();

        await consumer.StartAsync(
            async message =>
            {
                using var innerScope = scopeFactory.CreateScope();

                var useCase = innerScope.ServiceProvider
                    .GetRequiredService<IExecuteTransactionDailyBalanceService>();

                await useCase.ExecuteAsync(message);
            },
            stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}