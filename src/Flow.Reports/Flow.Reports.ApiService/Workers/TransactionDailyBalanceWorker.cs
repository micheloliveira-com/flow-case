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
        LogWorkerStarted();

        using var scope = scopeFactory.CreateScope();

        var consumer = ResolveConsumer(scope);

        await StartConsumerAsync(
            consumer,
            stoppingToken);

        await KeepWorkerAliveAsync(stoppingToken);
    }

    private void LogWorkerStarted()
    {
        logger.LogInformation(
            "Starting transaction daily balance worker");
    }

    private static ITransactionDailyBalanceConsumer ResolveConsumer(IServiceScope scope)
    {
        return scope.ServiceProvider
            .GetRequiredService<ITransactionDailyBalanceConsumer>();
    }

    private async Task StartConsumerAsync(
        ITransactionDailyBalanceConsumer consumer,
        CancellationToken stoppingToken)
    {
        await consumer.StartAsync(
            ProcessMessageAsync,
            stoppingToken);
    }

    private async Task ProcessMessageAsync(
        Shared.Application.Abstractions.Messaging.TransactionDailyBalanceMessage message)
    {
        LogMessageStarted(message.Date);

        using var scope = scopeFactory.CreateScope();

        var useCase = ResolveUseCase(scope);

        await useCase.ExecuteAsync(message);

        LogMessageFinished(message.Date);
    }

    private static IExecuteTransactionDailyBalanceService ResolveUseCase(
        IServiceScope scope)
    {
        return scope.ServiceProvider
            .GetRequiredService<IExecuteTransactionDailyBalanceService>();
    }

    private void LogMessageStarted(DateOnly date)
    {
        logger.LogInformation(
            "Received transaction daily balance message for {Date}",
            date);
    }

    private void LogMessageFinished(DateOnly date)
    {
        logger.LogInformation(
            "Finished transaction daily balance message for {Date}",
            date);
    }

    private static async Task KeepWorkerAliveAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
