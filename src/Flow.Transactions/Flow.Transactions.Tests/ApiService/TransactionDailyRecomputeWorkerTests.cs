using Flow.Transactions.ApiService.Workers;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;
using Flow.Transactions.Infrastructure.Messaging.Consumers;
using Flow.Shared.Infrastructure.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Transactions.Tests.ApiService;

public sealed class TransactionDailyRecomputeWorkerTests
{
    [Fact]
    public async Task StartAsync_WhenMessageIsConsumed_ShouldExecuteUseCaseInScope()
    {
        // Arrange
        var consumer = new TransactionDailyRecomputeConsumerFake();
        var useCase = new ExecuteTransactionDailyRecomputeServiceFake();

        await using var serviceProvider = new ServiceCollection()
            .AddSingleton<ITransactionDailyRecomputeConsumer>(consumer)
            .AddSingleton<IExecuteTransactionDailyRecomputeService>(useCase)
            .BuildServiceProvider();

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var worker = new TransactionDailyRecomputeWorker(scopeFactory);
        var message = new TransactionDailyRecomputeMessage(new DateOnly(2026, 5, 24));

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

    private sealed class TransactionDailyRecomputeConsumerFake : ITransactionDailyRecomputeConsumer
    {
        private readonly TaskCompletionSource started = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        private MessageHandler<TransactionDailyRecomputeMessage>? handler;

        public int StartCalls { get; private set; }

        public Task StartAsync(
            MessageHandler<TransactionDailyRecomputeMessage> handler,
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

        public Task DispatchAsync(TransactionDailyRecomputeMessage message)
        {
            Assert.NotNull(handler);
            return handler(message);
        }
    }

    private sealed class ExecuteTransactionDailyRecomputeServiceFake
        : IExecuteTransactionDailyRecomputeService
    {
        public int ExecuteCalls { get; private set; }

        public TransactionDailyRecomputeMessage? ExecutedMessage { get; private set; }

        public Task ExecuteAsync(TransactionDailyRecomputeMessage message)
        {
            ExecuteCalls++;
            ExecutedMessage = message;
            return Task.CompletedTask;
        }
    }
}
