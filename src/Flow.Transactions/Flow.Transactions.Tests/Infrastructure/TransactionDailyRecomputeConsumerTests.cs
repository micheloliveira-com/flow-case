using Flow.Shared.Infrastructure.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Infrastructure.Messaging.Consumers;
using Moq;

namespace Flow.Transactions.Tests.Infrastructure;

public sealed class TransactionDailyRecomputeConsumerTests
{
    [Fact]
    public async Task StartAsync_ShouldSubscribeToTransactionDailyRecomputeQueue()
    {
        // Arrange
        var consumer = new Mock<IMessageConsumer>(MockBehavior.Strict);
        MessageHandler<TransactionDailyRecomputeMessage> handler = _ => Task.CompletedTask;
        using var cancellationTokenSource = new CancellationTokenSource();

        consumer
            .Setup(x => x.SubscribeAsync(
                "transaction-daily-recompute",
                handler,
                cancellationTokenSource.Token))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var sut = new TransactionDailyRecomputeConsumer(consumer.Object);

        // Act
        await sut.StartAsync(handler, cancellationTokenSource.Token);

        // Assert
        consumer.Verify();
        consumer.VerifyNoOtherCalls();
    }
}
