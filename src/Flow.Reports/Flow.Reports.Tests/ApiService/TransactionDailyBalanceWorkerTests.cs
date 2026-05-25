using Flow.Reports.ApiService.Workers;
using Flow.Reports.Application.UseCases.DailyBalance.ExecuteTransactionDailyBalance;
using Flow.Reports.Infrastructure.Messaging.Consumers;
using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Shared.Infrastructure.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Reports.Tests.ApiService;

public sealed class TransactionDailyBalanceWorkerTests
{
    [Fact]
    public async Task StartAsync_WhenMessageIsConsumed_ShouldExecuteUseCaseInScope()
    {
        // Arrange
        var consumer = new TransactionDailyBalanceConsumerFake();
        var useCase = new ExecuteTransactionDailyBalanceServiceFake();

        await using var serviceProvider = new ServiceCollection()
            .AddSingleton<ITransactionDailyBalanceConsumer>(consumer)
            .AddSingleton<IExecuteTransactionDailyBalanceService>(useCase)
            .BuildServiceProvider();

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var worker = new TransactionDailyBalanceWorker(scopeFactory);
        var message = new TransactionDailyBalanceMessage(
            new DateOnly(2026, 5, 24),
            150m,
            new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc));

        // Act
        await worker.StartAsync(CancellationToken.None);
        await consumer.WaitUntilStartedAsync();
        await consumer.DispatchAsync(message);
        await worker.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(1, consumer.StartCalls);
        Assert.Same(message, useCase.ExecutedMessage);
        Assert.Equal(1, useCase.ExecuteCalls);
    }

    private sealed class TransactionDailyBalanceConsumerFake : ITransactionDailyBalanceConsumer
    {
        private readonly TaskCompletionSource started = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        private MessageHandler<TransactionDailyBalanceMessage>? handler;

        public int StartCalls { get; private set; }

        public Task StartAsync(
            MessageHandler<TransactionDailyBalanceMessage> handler,
            CancellationToken cancellationToken)
        {
            StartCalls++;
            this.handler = handler;
            started.SetResult();
            return Task.CompletedTask;
        }

        public Task WaitUntilStartedAsync()
        {
            return started.Task.WaitAsync(TimeSpan.FromSeconds(1));
        }

        public Task DispatchAsync(TransactionDailyBalanceMessage message)
        {
            Assert.NotNull(handler);
            return handler(message);
        }
    }

    private sealed class ExecuteTransactionDailyBalanceServiceFake
        : IExecuteTransactionDailyBalanceService
    {
        public int ExecuteCalls { get; private set; }

        public TransactionDailyBalanceMessage? ExecutedMessage { get; private set; }

        public Task ExecuteAsync(TransactionDailyBalanceMessage message)
        {
            ExecuteCalls++;
            ExecutedMessage = message;
            return Task.CompletedTask;
        }
    }
}
