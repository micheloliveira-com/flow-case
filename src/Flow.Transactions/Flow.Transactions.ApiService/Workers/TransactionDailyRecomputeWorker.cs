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
            "Starting transaction daily recompute worker");
    }

    private static ITransactionDailyRecomputeConsumer ResolveConsumer(
        IServiceScope scope)
    {
        return scope.ServiceProvider
            .GetRequiredService<ITransactionDailyRecomputeConsumer>();
    }

    private async Task StartConsumerAsync(
        ITransactionDailyRecomputeConsumer consumer,
        CancellationToken stoppingToken)
    {
        await consumer.StartAsync(
            async message => await ProcessMessageAsync(message),
            stoppingToken);
    }

    private async Task ProcessMessageAsync(
        Application.Abstractions.Messaging.TransactionDailyRecompute.TransactionDailyRecomputeMessage message)
    {
        LogMessageStarted(message.Date);

        using var scope = scopeFactory.CreateScope();

        var useCase = ResolveUseCase(scope);

        await useCase.ExecuteAsync(message);

        LogMessageFinished(message.Date);
    }

    private static IExecuteTransactionDailyRecomputeService ResolveUseCase(
        IServiceScope scope)
    {
        return scope.ServiceProvider
            .GetRequiredService<IExecuteTransactionDailyRecomputeService>();
    }

    private void LogMessageStarted(DateOnly date)
    {
        logger.LogInformation(
            "Received transaction daily recompute message for {Date}",
            date);
    }

    private void LogMessageFinished(DateOnly date)
    {
        logger.LogInformation(
            "Finished transaction daily recompute message for {Date}",
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
