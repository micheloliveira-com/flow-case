using Flow.Reports.Application.Abstractions.Persistence;
using Flow.Reports.Application.UseCases.DailyBalance.ExecuteTransactionDailyBalance;
using Flow.Reports.Domain.Entities;
using Flow.Shared.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Flow.Reports.Tests.Application;

public sealed class ExecuteTransactionDailyBalanceServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenBalanceDoesNotExist_ShouldAddAndSave()
    {
        // Arrange
        var repository = new TransactionDailyBalanceRepositoryFake();
        var service = CreateService(repository);
        var message = CreateMessage();

        // Act
        await service.ExecuteAsync(message);

        // Assert
        Assert.Equal(1, repository.GetByDateCalls);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);

        var added = Assert.Single(repository.AddedEntities);
        Assert.Equal(message.Date, added.Date);
        Assert.Equal(message.Balance, added.Balance);
        Assert.Equal(message.ProcessedAt, added.ProcessedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIncomingMessageIsNewer_ShouldUpdateCurrentAndSave()
    {
        // Arrange
        var current = new TransactionDailyBalance(
            new DateOnly(2026, 5, 24),
            100m,
            new DateTime(2026, 5, 24, 9, 0, 0, DateTimeKind.Utc));

        var repository = new TransactionDailyBalanceRepositoryFake(current);
        var service = CreateService(repository);
        var message = CreateMessage(
            balance: 250m,
            processedAt: new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc));

        // Act
        await service.ExecuteAsync(message);

        // Assert
        Assert.Equal(1, repository.GetByDateCalls);
        Assert.Equal(0, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
        Assert.Equal(message.Balance, current.Balance);
        Assert.Equal(message.ProcessedAt, current.ProcessedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIncomingMessageHasSameProcessedAt_ShouldIgnoreAndNotSave()
    {
        // Arrange
        var processedAt = new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc);
        var current = new TransactionDailyBalance(
            new DateOnly(2026, 5, 24),
            100m,
            processedAt);

        var repository = new TransactionDailyBalanceRepositoryFake(current);
        var service = CreateService(repository);
        var message = CreateMessage(balance: 250m, processedAt: processedAt);

        // Act
        await service.ExecuteAsync(message);

        // Assert
        Assert.Equal(1, repository.GetByDateCalls);
        Assert.Equal(0, repository.AddCalls);
        Assert.Equal(0, repository.SaveChangesCalls);
        Assert.Equal(100m, current.Balance);
        Assert.Equal(processedAt, current.ProcessedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIncomingMessageIsOlder_ShouldIgnoreAndNotSave()
    {
        // Arrange
        var currentProcessedAt = new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc);
        var current = new TransactionDailyBalance(
            new DateOnly(2026, 5, 24),
            100m,
            currentProcessedAt);

        var repository = new TransactionDailyBalanceRepositoryFake(current);
        var service = CreateService(repository);
        var message = CreateMessage(
            balance: 250m,
            processedAt: new DateTime(2026, 5, 24, 9, 59, 59, DateTimeKind.Utc));

        // Act
        await service.ExecuteAsync(message);

        // Assert
        Assert.Equal(1, repository.GetByDateCalls);
        Assert.Equal(0, repository.AddCalls);
        Assert.Equal(0, repository.SaveChangesCalls);
        Assert.Equal(100m, current.Balance);
        Assert.Equal(currentProcessedAt, current.ProcessedAt);
    }

    private static ExecuteTransactionDailyBalanceService CreateService(
        TransactionDailyBalanceRepositoryFake repository)
    {
        return new ExecuteTransactionDailyBalanceService(
            repository,
            NullLogger<ExecuteTransactionDailyBalanceService>.Instance);
    }

    private static TransactionDailyBalanceMessage CreateMessage(
        decimal balance = 150m,
        DateTime? processedAt = null)
    {
        return new TransactionDailyBalanceMessage(
            new DateOnly(2026, 5, 24),
            balance,
            processedAt ?? new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc));
    }

    private sealed class TransactionDailyBalanceRepositoryFake(
        TransactionDailyBalance? current = null)
        : ITransactionDailyBalanceRepository
    {
        public int AddCalls { get; private set; }

        public int GetByDateCalls { get; private set; }

        public int SaveChangesCalls { get; private set; }

        public List<TransactionDailyBalance> AddedEntities { get; } = [];

        public Task AddAsync(TransactionDailyBalance entity)
        {
            AddCalls++;
            AddedEntities.Add(entity);
            return Task.CompletedTask;
        }

        public Task<List<TransactionDailyBalance>> GetAsync(DateOnly? start, DateOnly? end)
        {
            throw new NotSupportedException();
        }

        public Task<TransactionDailyBalance?> GetByDateAsync(DateOnly date)
        {
            GetByDateCalls++;
            return Task.FromResult(current);
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
