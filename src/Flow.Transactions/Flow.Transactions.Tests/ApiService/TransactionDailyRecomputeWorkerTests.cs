using Flow.Transactions.ApiService.Workers;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;
using Flow.Transactions.Infrastructure.Messaging.Consumers;
using Flow.Shared.Infrastructure.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Flow.Transactions.Tests.ApiService;

public sealed class TransactionDailyRecomputeWorkerTests
{
    [Fact]
    public async Task StartAsync_WhenMessageIsConsumed_ShouldExecuteUseCaseInScope()
    {
        // Arrange
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        MessageHandler<TransactionDailyRecomputeMessage>? handler = null;

        var consumer = new Mock<ITransactionDailyRecomputeConsumer>(MockBehavior.Strict);
        var useCase = new Mock<IExecuteTransactionDailyRecomputeService>(MockBehavior.Strict);

        await using var serviceProvider = new ServiceCollection()
            .AddSingleton(consumer.Object)
            .AddSingleton(useCase.Object)
            .BuildServiceProvider();

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var worker = new TransactionDailyRecomputeWorker(scopeFactory);
        var message = new TransactionDailyRecomputeMessage(new DateOnly(2026, 5, 24));

        consumer
            .Setup(x => x.StartAsync(
                It.IsAny<MessageHandler<TransactionDailyRecomputeMessage>>(),
                It.IsAny<CancellationToken>()))
            .Callback<MessageHandler<TransactionDailyRecomputeMessage>, CancellationToken>((capturedHandler, _) =>
            {
                handler = capturedHandler;
                started.SetResult();
            })
            .Returns(Task.CompletedTask)
            .Verifiable();

        useCase
            .Setup(x => x.ExecuteAsync(message))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await worker.StartAsync(CancellationToken.None);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.NotNull(handler);
        await handler(message);
        await worker.StopAsync(CancellationToken.None);

        // Assert
        consumer.Verify();
        useCase.Verify();
        consumer.VerifyNoOtherCalls();
        useCase.VerifyNoOtherCalls();
    }
}
