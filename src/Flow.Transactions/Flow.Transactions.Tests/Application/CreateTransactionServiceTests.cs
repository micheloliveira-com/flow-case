using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.UseCases.Transactions.CreateTransaction;
using Flow.Transactions.Domain.Entities;
using Flow.Transactions.Domain.Entities.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Flow.Transactions.Tests.Application;

public sealed class CreateTransactionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenRequestIsValid_ShouldAddSavePublishAndReturnTransaction()
    {
        // Arrange
        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        var publisher = new Mock<ITransactionDailyRecomputePublisher>(MockBehavior.Strict);
        var request = new CreateTransactionRequest(
            150m,
            TransactionType.Credit,
            new DateOnly(2026, 5, 24),
            "Salary");

        repository
            .Setup(x => x.AddAsync(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        repository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();

        publisher
            .Setup(x => x.PublishAsync(
                It.Is<TransactionDailyRecomputeMessage>(message => message.Date == request.Date)))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = new CreateTransactionService(
            repository.Object,
            publisher.Object,
            NullLogger<CreateTransactionService>.Instance);

        // Act
        var actual = await service.ExecuteAsync(request);

        // Assert
        Assert.Equal(request.Amount, actual.Amount);
        Assert.Equal(request.Type, actual.Type);
        Assert.Equal(request.Date, actual.Date);
        Assert.Equal(request.Description, actual.Description);

        repository.Verify(x => x.AddAsync(actual), Times.Once);
        repository.Verify(x => x.SaveChangesAsync(), Times.Once);
        publisher.Verify();
        repository.VerifyNoOtherCalls();
        publisher.VerifyNoOtherCalls();
    }
}
