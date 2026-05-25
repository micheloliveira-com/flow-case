using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyBalance;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;
using Flow.Transactions.Domain.Entities;
using Flow.Transactions.Domain.Entities.Enums;
using Moq;

namespace Flow.Transactions.Tests.Application;

public sealed class ExecuteTransactionDailyRecomputeServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPublishCreditMinusDebitBalanceForMessageDate()
    {
        // Arrange
        var date = new DateOnly(2026, 5, 24);
        var transactions = new List<Transaction>
        {
            new(100m, TransactionType.Credit, date, "Salary"),
            new(25m, TransactionType.Debit, date, "Lunch"),
            new(10m, TransactionType.Credit, date, "Cashback")
        };

        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var publisher = new Mock<ITransactionDailyBalancePublisher>(MockBehavior.Strict);

        repository
            .Setup(x => x.GetByDateAsync(date))
            .ReturnsAsync(transactions)
            .Verifiable();

        publisher
            .Setup(x => x.PublishAsync(It.Is<TransactionDailyBalanceMessage>(message =>
                message.Date == date &&
                message.Balance == 85m &&
                message.ProcessedAt <= DateTime.UtcNow &&
                message.ProcessedAt >= DateTime.UtcNow.AddMinutes(-1))))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new ExecuteTransactionDailyRecomputeService(repository.Object, publisher.Object);

        // Act
        await service.ExecuteAsync(new TransactionDailyRecomputeMessage(date));

        // Assert
        repository.Verify();
        publisher.Verify();
        repository.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenThereAreNoTransactions_ShouldPublishZeroBalance()
    {
        // Arrange
        var date = new DateOnly(2026, 5, 24);
        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var publisher = new Mock<ITransactionDailyBalancePublisher>(MockBehavior.Strict);

        repository
            .Setup(x => x.GetByDateAsync(date))
            .ReturnsAsync([])
            .Verifiable();

        publisher
            .Setup(x => x.PublishAsync(It.Is<TransactionDailyBalanceMessage>(message =>
                message.Date == date &&
                message.Balance == 0m)))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new ExecuteTransactionDailyRecomputeService(repository.Object, publisher.Object);

        // Act
        await service.ExecuteAsync(new TransactionDailyRecomputeMessage(date));

        // Assert
        repository.Verify();
        publisher.Verify();
        repository.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }
}
