using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.UseCases.Transactions.DeleteTransaction;
using Flow.Transactions.Domain.Entities;
using Flow.Transactions.Domain.Entities.Enums;
using Moq;

namespace Flow.Transactions.Tests.Application;

public sealed class DeleteTransactionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenTransactionDoesNotExist_ShouldReturnFalseAndNotPublish()
    {
        // Arrange
        var id = Guid.NewGuid();
        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var publisher = new Mock<ITransactionDailyRecomputePublisher>(MockBehavior.Strict);

        repository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((Transaction?)null)
            .Verifiable();

        var service = new DeleteTransactionService(repository.Object, publisher.Object);

        // Act
        var actual = await service.ExecuteAsync(new DeleteTransactionRequest(id));

        // Assert
        Assert.False(actual);
        repository.Verify();
        repository.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTransactionExists_ShouldRemoveSavePublishAndReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = new DateOnly(2026, 5, 24);
        var transaction = CreateTransaction(id, date);
        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var publisher = new Mock<ITransactionDailyRecomputePublisher>(MockBehavior.Strict);

        repository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(transaction)
            .Verifiable();

        repository
            .Setup(x => x.Remove(transaction))
            .Verifiable();

        repository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();

        publisher
            .Setup(x => x.PublishAsync(
                It.Is<TransactionDailyRecomputeMessage>(message => message.Date == date)))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new DeleteTransactionService(repository.Object, publisher.Object);

        // Act
        var actual = await service.ExecuteAsync(new DeleteTransactionRequest(id));

        // Assert
        Assert.True(actual);
        repository.Verify();
        publisher.Verify();
        repository.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    private static Transaction CreateTransaction(Guid id, DateOnly date)
    {
        return new Transaction(id, 150m, TransactionType.Credit, date, "Salary");
    }
}
