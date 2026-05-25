using Flow.Reports.Application.Abstractions.Persistence;
using Flow.Reports.Application.UseCases.DailyBalance.ExecuteTransactionDailyBalance;
using Flow.Reports.Domain.Entities;
using Flow.Shared.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Flow.Reports.Tests.Application;

public sealed class ExecuteTransactionDailyBalanceServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenBalanceDoesNotExist_ShouldAddAndSave()
    {
        // Arrange
        var repository = new Mock<ITransactionDailyBalanceRepository>(MockBehavior.Strict);
        var message = CreateMessage();

        repository
            .Setup(x => x.GetByDateAsync(message.Date))
            .ReturnsAsync((TransactionDailyBalance?)null)
            .Verifiable();

        repository
            .Setup(x => x.AddAsync(It.Is<TransactionDailyBalance>(entity =>
                entity.Date == message.Date &&
                entity.Balance == message.Balance &&
                entity.ProcessedAt == message.ProcessedAt)))
            .Returns(Task.CompletedTask)
            .Verifiable();

        repository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = CreateService(repository.Object);

        // Act
        await service.ExecuteAsync(message);

        // Assert
        repository.Verify();
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenIncomingMessageIsNewer_ShouldUpdateCurrentAndSave()
    {
        // Arrange
        var current = new TransactionDailyBalance(
            new DateOnly(2026, 5, 24),
            100m,
            new DateTime(2026, 5, 24, 9, 0, 0, DateTimeKind.Utc));

        var repository = new Mock<ITransactionDailyBalanceRepository>(MockBehavior.Strict);
        var message = CreateMessage(
            balance: 250m,
            processedAt: new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc));

        repository
            .Setup(x => x.GetByDateAsync(message.Date))
            .ReturnsAsync(current)
            .Verifiable();

        repository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();

        var service = CreateService(repository.Object);

        // Act
        await service.ExecuteAsync(message);

        // Assert
        Assert.Equal(message.Balance, current.Balance);
        Assert.Equal(message.ProcessedAt, current.ProcessedAt);
        repository.Verify();
        repository.VerifyNoOtherCalls();
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

        var repository = new Mock<ITransactionDailyBalanceRepository>(MockBehavior.Strict);
        var message = CreateMessage(balance: 250m, processedAt: processedAt);

        repository
            .Setup(x => x.GetByDateAsync(message.Date))
            .ReturnsAsync(current)
            .Verifiable();

        var service = CreateService(repository.Object);

        // Act
        await service.ExecuteAsync(message);

        // Assert
        Assert.Equal(100m, current.Balance);
        Assert.Equal(processedAt, current.ProcessedAt);
        repository.Verify();
        repository.VerifyNoOtherCalls();
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

        var repository = new Mock<ITransactionDailyBalanceRepository>(MockBehavior.Strict);
        var message = CreateMessage(
            balance: 250m,
            processedAt: new DateTime(2026, 5, 24, 9, 59, 59, DateTimeKind.Utc));

        repository
            .Setup(x => x.GetByDateAsync(message.Date))
            .ReturnsAsync(current)
            .Verifiable();

        var service = CreateService(repository.Object);

        // Act
        await service.ExecuteAsync(message);

        // Assert
        Assert.Equal(100m, current.Balance);
        Assert.Equal(currentProcessedAt, current.ProcessedAt);
        repository.Verify();
        repository.VerifyNoOtherCalls();
    }

    private static ExecuteTransactionDailyBalanceService CreateService(
        ITransactionDailyBalanceRepository repository)
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
}
