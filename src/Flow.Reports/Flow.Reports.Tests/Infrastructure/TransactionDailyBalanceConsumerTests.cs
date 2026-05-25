using Flow.Reports.Infrastructure.Messaging.Consumers;
using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Shared.Infrastructure.Abstractions.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Flow.Reports.Tests.Infrastructure;

public sealed class TransactionDailyBalanceConsumerTests
{
    [Fact]
    public async Task StartAsync_ShouldSubscribeToTransactionDailyBalanceQueue()
    {
        // Arrange
        var consumer = new Mock<IMessageConsumer>(MockBehavior.Strict);
        MessageHandler<TransactionDailyBalanceMessage> handler = _ => Task.CompletedTask;
        using var cancellationTokenSource = new CancellationTokenSource();

        consumer
            .Setup(x => x.SubscribeAsync(
                "transaction-daily-balance",
                handler,
                cancellationTokenSource.Token))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var sut = new TransactionDailyBalanceConsumer(
            consumer.Object,
            NullLogger<TransactionDailyBalanceConsumer>.Instance);

        // Act
        await sut.StartAsync(handler, cancellationTokenSource.Token);

        // Assert
        consumer.Verify();
        consumer.VerifyNoOtherCalls();
    }
}
