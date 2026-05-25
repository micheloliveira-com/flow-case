using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;
using Flow.Transactions.Domain.Entities;
using Flow.Transactions.Domain.Entities.Enums;
using Moq;

namespace Flow.Transactions.Tests.Application;

public sealed class UpdateTransactionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenTransactionDoesNotExist_ShouldReturnNullAndNotPublish()
    {
        // Arrange
        var id = Guid.NewGuid();
        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var publisher = new Mock<ITransactionDailyRecomputePublisher>(MockBehavior.Strict);

        repository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((Transaction?)null)
            .Verifiable();

        var service = new UpdateTransactionService(repository.Object, publisher.Object);

        // Act
        var actual = await service.ExecuteAsync(id, CreateRequest(new DateOnly(2026, 5, 24)));

        // Assert
        Assert.Null(actual);
        repository.Verify();
        repository.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDateDoesNotChange_ShouldUpdateSavePublishOnceAndReturnUpdatedTransaction()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = new DateOnly(2026, 5, 24);
        var current = CreateTransaction(id, date);
        var request = CreateRequest(date);
        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var publisher = new Mock<ITransactionDailyRecomputePublisher>(MockBehavior.Strict);

        repository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(current)
            .Verifiable();

        repository
            .Setup(x => x.UpdateAsync(It.Is<Transaction>(transaction =>
                transaction.Id == id &&
                transaction.Amount == request.Amount &&
                transaction.Type == request.Type &&
                transaction.Date == request.Date &&
                transaction.Description == request.Description)))
            .Returns(Task.CompletedTask)
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

        var service = new UpdateTransactionService(repository.Object, publisher.Object);

        // Act
        var actual = await service.ExecuteAsync(id, request);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(id, actual.Id);
        Assert.Equal(request.Amount, actual.Amount);
        Assert.Equal(request.Type, actual.Type);
        Assert.Equal(request.Date, actual.Date);
        Assert.Equal(request.Description, actual.Description);

        repository.Verify();
        publisher.Verify(x => x.PublishAsync(It.IsAny<TransactionDailyRecomputeMessage>()), Times.Once);
        repository.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDateChanges_ShouldPublishRecomputeForOldAndNewDates()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldDate = new DateOnly(2026, 5, 23);
        var newDate = new DateOnly(2026, 5, 24);
        var current = CreateTransaction(id, oldDate);
        var request = CreateRequest(newDate);
        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var publisher = new Mock<ITransactionDailyRecomputePublisher>(MockBehavior.Strict);

        repository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(current)
            .Verifiable();

        repository
            .Setup(x => x.UpdateAsync(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        repository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();

        publisher
            .Setup(x => x.PublishAsync(
                It.Is<TransactionDailyRecomputeMessage>(message => message.Date == oldDate)))
            .Returns(Task.CompletedTask)
            .Verifiable();

        publisher
            .Setup(x => x.PublishAsync(
                It.Is<TransactionDailyRecomputeMessage>(message => message.Date == newDate)))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new UpdateTransactionService(repository.Object, publisher.Object);

        // Act
        await service.ExecuteAsync(id, request);

        // Assert
        repository.Verify();
        publisher.Verify();
        publisher.Verify(x => x.PublishAsync(It.IsAny<TransactionDailyRecomputeMessage>()), Times.Exactly(2));
        repository.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }

    private static UpdateTransactionRequest CreateRequest(DateOnly date)
    {
        return new UpdateTransactionRequest(250m, TransactionType.Debit, date, "Rent");
    }

    private static Transaction CreateTransaction(Guid id, DateOnly date)
    {
        return new Transaction(id, 150m, TransactionType.Credit, date, "Salary");
    }
}
