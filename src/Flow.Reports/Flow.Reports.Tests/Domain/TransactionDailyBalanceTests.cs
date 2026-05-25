using Flow.Reports.Domain.Entities;

namespace Flow.Reports.Tests.Domain;

public sealed class TransactionDailyBalanceTests
{
    [Fact]
    public void Constructor_WhenInputIsValid_ShouldCreateTransactionDailyBalance()
    {
        // Arrange
        var date = new DateOnly(2026, 5, 24);
        var balance = 150.75m;
        var processedAt = new DateTime(2026, 5, 24, 12, 30, 0, DateTimeKind.Utc);

        // Act
        var entity = new TransactionDailyBalance(date, balance, processedAt);

        // Assert
        Assert.Equal(date, entity.Date);
        Assert.Equal(balance, entity.Balance);
        Assert.Equal(processedAt, entity.ProcessedAt);
    }

    [Fact]
    public void Constructor_WhenDateIsDefault_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var processedAt = new DateTime(2026, 5, 24, 12, 30, 0, DateTimeKind.Utc);

        // Act
        var act = () => new TransactionDailyBalance(default, 150.75m, processedAt);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Invalid date", exception.Message);
    }

    [Fact]
    public void Constructor_WhenProcessedAtIsDefault_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var date = new DateOnly(2026, 5, 24);

        // Act
        var act = () => new TransactionDailyBalance(date, 150.75m, default);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Invalid processed date", exception.Message);
    }

    [Fact]
    public void Apply_WhenInputIsValid_ShouldUpdateBalanceAndProcessedAt()
    {
        // Arrange
        var entity = new TransactionDailyBalance(
            new DateOnly(2026, 5, 24),
            150.75m,
            new DateTime(2026, 5, 24, 12, 30, 0, DateTimeKind.Utc));

        var newBalance = 250.25m;
        var newProcessedAt = new DateTime(2026, 5, 24, 13, 45, 0, DateTimeKind.Utc);

        // Act
        entity.Apply(newBalance, newProcessedAt);

        // Assert
        Assert.Equal(newBalance, entity.Balance);
        Assert.Equal(newProcessedAt, entity.ProcessedAt);
    }

    [Fact]
    public void Apply_WhenProcessedAtIsDefault_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var originalProcessedAt = new DateTime(2026, 5, 24, 12, 30, 0, DateTimeKind.Utc);
        var entity = new TransactionDailyBalance(
            new DateOnly(2026, 5, 24),
            150.75m,
            originalProcessedAt);

        // Act
        var act = () => entity.Apply(250.25m, default);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Invalid processed date", exception.Message);
        Assert.Equal(150.75m, entity.Balance);
        Assert.Equal(originalProcessedAt, entity.ProcessedAt);
    }
}
