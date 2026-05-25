using Flow.Reports.Application.Abstractions.Persistence;
using Flow.Reports.Application.UseCases.TransactionDailyBalance.GetTransactionDailyBalance;
using Flow.Reports.Domain.Entities;

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

        var repository = new TransactionDailyBalanceRepositoryFake(expected);
        var service = new GetTransactionDailyBalance(repository);
        var request = new GetTransactionDailyBalanceRequest(
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 31));

        // Act
        var actual = await service.ExecuteAsync(request);

        // Assert
        Assert.Same(expected, actual);
        Assert.Equal(request.Start, repository.Start);
        Assert.Equal(request.End, repository.End);
        Assert.Equal(1, repository.GetCalls);
    }

    private sealed class TransactionDailyBalanceRepositoryFake(
        List<TransactionDailyBalance> result)
        : ITransactionDailyBalanceRepository
    {
        public int GetCalls { get; private set; }

        public DateOnly? Start { get; private set; }

        public DateOnly? End { get; private set; }

        public Task AddAsync(TransactionDailyBalance entity)
        {
            throw new NotSupportedException();
        }

        public Task<List<TransactionDailyBalance>> GetAsync(DateOnly? start, DateOnly? end)
        {
            GetCalls++;
            Start = start;
            End = end;
            return Task.FromResult(result);
        }

        public Task<TransactionDailyBalance?> GetByDateAsync(DateOnly date)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotSupportedException();
        }
    }
}
