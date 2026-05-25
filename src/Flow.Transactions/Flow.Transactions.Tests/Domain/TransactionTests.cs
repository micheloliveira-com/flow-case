using Flow.Transactions.Domain.Entities;
using Flow.Transactions.Domain.Entities.Enums;

namespace Flow.Transactions.Tests.Domain;

public sealed class TransactionTests
{
    [Fact]
    public void Constructor_WhenInputIsValid_ShouldCreateTransaction()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = new DateOnly(2026, 5, 24);

        // Act
        var transaction = new Transaction(
            id,
            150.75m,
            TransactionType.Credit,
            date,
            "Salary");

        // Assert
        Assert.Equal(id, transaction.Id);
        Assert.Equal(150.75m, transaction.Amount);
        Assert.Equal(TransactionType.Credit, transaction.Type);
        Assert.Equal(date, transaction.Date);
        Assert.Equal("Salary", transaction.Description);
    }

    [Theory]
    [InlineData(0)]
    public void Constructor_WhenAmountIsZero_ShouldThrowInvalidOperationException(decimal amount)
    {
        // Act
        var act = () => CreateTransaction(amount: amount);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Amount must not be zero", exception.Message);
    }

    [Fact]
    public void Constructor_WhenTypeIsInvalid_ShouldThrowInvalidOperationException()
    {
        // Act
        var act = () => CreateTransaction(type: (TransactionType)999);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Invalid transaction type", exception.Message);
    }

    [Fact]
    public void Constructor_WhenDateIsDefault_ShouldThrowInvalidOperationException()
    {
        // Act
        var act = () => CreateTransaction(date: default(DateOnly));

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Invalid date", exception.Message);
    }

    [Fact]
    public void Constructor_WhenDescriptionIsLongerThan255Characters_ShouldThrowInvalidOperationException()
    {
        // Act
        var act = () => CreateTransaction(description: new string('A', 256));

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Description too long", exception.Message);
    }

    [Fact]
    public void Constructor_WhenDescriptionIsNull_ShouldCreateTransaction()
    {
        // Act
        var transaction = CreateTransaction(description: null);

        // Assert
        Assert.Null(transaction.Description);
    }

    private static Transaction CreateTransaction(
        decimal amount = 150.75m,
        TransactionType type = TransactionType.Credit,
        DateOnly? date = null,
        string? description = "Salary")
    {
        return new Transaction(
            amount,
            type,
            date ?? new DateOnly(2026, 5, 24),
            description);
    }
}
