using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.UseCases.Transactions.GetTransactions;
using Flow.Transactions.Domain.Entities;
using Flow.Transactions.Domain.Entities.Enums;
using Moq;

namespace Flow.Transactions.Tests.Application;

public sealed class GetTransactionsServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnRepositoryResultUsingRequestPeriod()
    {
        // Arrange
        var start = new DateOnly(2026, 5, 1);
        var end = new DateOnly(2026, 5, 31);
        var expected = new List<Transaction>
        {
            new(150m, TransactionType.Credit, new DateOnly(2026, 5, 24), "Salary")
        };

        var repository = new Mock<ITransactionRepository>(MockBehavior.Strict);
        repository
            .Setup(x => x.GetAsync(start, end))
            .ReturnsAsync(expected)
            .Verifiable();

        var service = new GetTransactionsService(repository.Object);

        // Act
        var actual = await service.ExecuteAsync(new GetTransactionsRequest(start, end));

        // Assert
        Assert.Same(expected, actual);
        repository.Verify();
        repository.VerifyNoOtherCalls();
    }
}
