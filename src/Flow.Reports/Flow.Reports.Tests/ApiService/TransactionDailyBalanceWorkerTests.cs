using Flow.Reports.ApiService.Workers;
using Flow.Reports.Application.UseCases.DailyBalance.ExecuteTransactionDailyBalance;
using Flow.Reports.Infrastructure.Messaging.Consumers;
using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Shared.Infrastructure.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Flow.Reports.Tests.ApiService;

public sealed class TransactionDailyBalanceWorkerTests
{
    [Fact]
    public async Task StartAsync_WhenMessageIsConsumed_ShouldExecuteUseCaseInScope()
    {
        // Arrange
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        MessageHandler<TransactionDailyBalanceMessage>? handler = null;

        var consumer = new Mock<ITransactionDailyBalanceConsumer>(MockBehavior.Strict);
        var useCase = new Mock<IExecuteTransactionDailyBalanceService>(MockBehavior.Strict);

        await using var serviceProvider = new ServiceCollection()
            .AddSingleton(consumer.Object)
            .AddSingleton(useCase.Object)
            .BuildServiceProvider();

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var worker = new TransactionDailyBalanceWorker(
            scopeFactory,
            NullLogger<TransactionDailyBalanceWorker>.Instance);
        var message = new TransactionDailyBalanceMessage(
            new DateOnly(2026, 5, 24),
            150m,
            new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc));

        consumer
            .Setup(x => x.StartAsync(
                It.IsAny<MessageHandler<TransactionDailyBalanceMessage>>(),
                It.IsAny<CancellationToken>()))
            .Callback<MessageHandler<TransactionDailyBalanceMessage>, CancellationToken>((capturedHandler, _) =>
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
