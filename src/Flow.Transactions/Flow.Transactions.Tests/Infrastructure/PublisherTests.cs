using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Infrastructure.Messaging;
using Flow.Transactions.Infrastructure.Messaging.Publishers;
using Moq;

namespace Flow.Transactions.Tests.Infrastructure;

public sealed class PublisherTests
{
    [Fact]
    public async Task TransactionDailyRecomputePublisher_ShouldPublishUsingTransactionDailyRecomputeRoutingKey()
    {
        // Arrange
        var publisher = new Mock<IMessagePublisher>(MockBehavior.Strict);
        var message = new TransactionDailyRecomputeMessage(new DateOnly(2026, 5, 24));

        publisher
            .Setup(x => x.PublishAsync("transaction-daily-recompute", message))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var sut = new TransactionDailyRecomputePublisher(publisher.Object);

        // Act
        await sut.PublishAsync(message);

        // Assert
        publisher.Verify();
        publisher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task TransactionDailyBalancePublisher_ShouldPublishUsingTransactionDailyBalanceRoutingKey()
    {
        // Arrange
        var publisher = new Mock<IMessagePublisher>(MockBehavior.Strict);
        var message = new TransactionDailyBalanceMessage(
            new DateOnly(2026, 5, 24),
            150m,
            new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc));

        publisher
            .Setup(x => x.PublishAsync("transaction-daily-balance", message))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var sut = new TransactionDailyBalancePublisher(publisher.Object);

        // Act
        await sut.PublishAsync(message);

        // Assert
        publisher.Verify();
        publisher.VerifyNoOtherCalls();
    }
}
