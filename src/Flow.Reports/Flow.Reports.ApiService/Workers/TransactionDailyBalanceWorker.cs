using Flow.Reports.Application.UseCases.DailyBalance.ExecuteTransactionDailyBalance;
using Flow.Reports.Infrastructure.Messaging.Consumers;

namespace Flow.Reports.ApiService.Workers;
public sealed class TransactionDailyBalanceWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TransactionDailyBalanceWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting transaction daily balance worker");
        using var scope = scopeFactory.CreateScope();

        var consumer = scope.ServiceProvider
            .GetRequiredService<ITransactionDailyBalanceConsumer>();

        await consumer.StartAsync(
            async message =>
            {
                logger.LogInformation(
                    "Received transaction daily balance message for {Date}",
                    message.Date);
                using var innerScope = scopeFactory.CreateScope();

                var useCase = innerScope.ServiceProvider
                    .GetRequiredService<IExecuteTransactionDailyBalanceService>();

                await useCase.ExecuteAsync(message);
                logger.LogInformation(
                    "Finished transaction daily balance message for {Date}",
                    message.Date);
            },
            stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
