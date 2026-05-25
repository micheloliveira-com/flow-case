using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;
using Flow.Transactions.Infrastructure.Messaging.Consumers;
using Flow.Shared.Infrastructure.Abstractions.Messaging;

namespace Flow.Transactions.ApiService.Workers;

public sealed class TransactionDailyRecomputeWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TransactionDailyRecomputeWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting transaction daily recompute worker");
        using var scope = scopeFactory.CreateScope();

        var consumer = scope.ServiceProvider
            .GetRequiredService<ITransactionDailyRecomputeConsumer>();

        await consumer.StartAsync(
            async message =>
            {
                logger.LogInformation(
                    "Received transaction daily recompute message for {Date}",
                    message.Date);
                using var innerScope = scopeFactory.CreateScope();

                var useCase = innerScope.ServiceProvider
                    .GetRequiredService<IExecuteTransactionDailyRecomputeService>();

                await useCase.ExecuteAsync(message);
                logger.LogInformation(
                    "Finished transaction daily recompute message for {Date}",
                    message.Date);
            },
            stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
