using Flow.Reports.Application.Abstractions.Persistence;
using Flow.Reports.Application.UseCases.TransactionDailyBalance.GetTransactionDailyBalance;
using Flow.Reports.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Flow.Reports.Tests.Application;

public sealed class GetTransactionDailyBalanceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnRepositoryResultUsingRequestPeriod()
    {
        // Arrange
        var expected = new List<TransactionDailyBalance>
        {
            new(
                new DateOnly(2026, 5, 24),
                150m,
                new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc))
        };

        var request = new GetTransactionDailyBalanceRequest(
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 31));

        var repository = new Mock<ITransactionDailyBalanceRepository>(MockBehavior.Strict);
        repository
            .Setup(x => x.GetAsync(request.Start, request.End))
            .ReturnsAsync(expected)
            .Verifiable();

        var service = new GetTransactionDailyBalance(
            repository.Object,
            NullLogger<GetTransactionDailyBalance>.Instance);

        // Act
        var actual = await service.ExecuteAsync(request);

        // Assert
        Assert.Same(expected, actual);
        repository.Verify();
        repository.VerifyNoOtherCalls();
    }
}
