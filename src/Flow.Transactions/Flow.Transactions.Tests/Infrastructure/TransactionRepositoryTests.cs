using Flow.Transactions.Domain.Entities;
using Flow.Transactions.Domain.Entities.Enums;
using Flow.Transactions.Infrastructure.Persistence;
using Flow.Transactions.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Flow.Transactions.Tests.Infrastructure;

public sealed class TransactionRepositoryTests
{
    [Fact]
    public async Task GetAsync_WhenNoPeriodIsProvided_ShouldReturnTransactionsOrderedByDateDescending()
    {
        // Arrange
        await using var db = CreateDbContext();
        await SeedAsync(
            db,
            CreateTransaction(new DateOnly(2026, 5, 22), 100m),
            CreateTransaction(new DateOnly(2026, 5, 24), 300m),
            CreateTransaction(new DateOnly(2026, 5, 23), 200m));

        var repository = new TransactionRepository(db);

        // Act
        var actual = await repository.GetAsync(start: null, end: null);

        // Assert
        Assert.Collection(
            actual,
            item => Assert.Equal(new DateOnly(2026, 5, 24), item.Date),
            item => Assert.Equal(new DateOnly(2026, 5, 23), item.Date),
            item => Assert.Equal(new DateOnly(2026, 5, 22), item.Date));
    }

    [Fact]
    public async Task GetAsync_WhenPeriodIsProvided_ShouldApplyInclusiveDateFilters()
    {
        // Arrange
        await using var db = CreateDbContext();
        await SeedAsync(
            db,
            CreateTransaction(new DateOnly(2026, 5, 20), 100m),
            CreateTransaction(new DateOnly(2026, 5, 21), 200m),
            CreateTransaction(new DateOnly(2026, 5, 23), 300m),
            CreateTransaction(new DateOnly(2026, 5, 24), 400m));

        var repository = new TransactionRepository(db);

        // Act
        var actual = await repository.GetAsync(
            new DateOnly(2026, 5, 21),
            new DateOnly(2026, 5, 23));

        // Assert
        Assert.Collection(
            actual,
            item => Assert.Equal(new DateOnly(2026, 5, 23), item.Date),
            item => Assert.Equal(new DateOnly(2026, 5, 21), item.Date));
    }

    [Fact]
    public async Task GetByDateAsync_ShouldReturnOnlyTransactionsFromDate()
    {
        // Arrange
        await using var db = CreateDbContext();
        var expectedDate = new DateOnly(2026, 5, 24);
        await SeedAsync(
            db,
            CreateTransaction(expectedDate, 100m),
            CreateTransaction(expectedDate, 200m),
            CreateTransaction(new DateOnly(2026, 5, 23), 300m));

        var repository = new TransactionRepository(db);

        // Act
        var actual = await repository.GetByDateAsync(expectedDate);

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.All(actual, item => Assert.Equal(expectedDate, item.Date));
    }

    [Fact]
    public async Task GetByIdAsync_WhenTransactionExists_ShouldReturnTrackedEntity()
    {
        // Arrange
        await using var db = CreateDbContext();
        var id = Guid.NewGuid();
        var expected = CreateTransaction(id, new DateOnly(2026, 5, 24), 100m);
        await SeedAsync(db, expected);

        var repository = new TransactionRepository(db);

        // Act
        var actual = await repository.GetByIdAsync(id);

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task AddAsyncAndSaveChangesAsync_ShouldPersistTransaction()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new TransactionRepository(db);
        var transaction = CreateTransaction(new DateOnly(2026, 5, 24), 100m);

        // Act
        await repository.AddAsync(transaction);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await db.Transactions.SingleAsync();
        Assert.Equal(transaction.Amount, persisted.Amount);
        Assert.Equal(transaction.Type, persisted.Type);
        Assert.Equal(transaction.Date, persisted.Date);
        Assert.Equal(transaction.Description, persisted.Description);
    }

    [Fact]
    public async Task RemoveAndSaveChangesAsync_ShouldDeleteTransaction()
    {
        // Arrange
        await using var db = CreateDbContext();
        var transaction = CreateTransaction(new DateOnly(2026, 5, 24), 100m);
        await SeedAsync(db, transaction);

        var repository = new TransactionRepository(db);

        // Act
        repository.Remove(transaction);
        await repository.SaveChangesAsync();

        // Assert
        Assert.Empty(await db.Transactions.ToListAsync());
    }

    private static TransactionDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TransactionDbContext(options);
    }

    private static Transaction CreateTransaction(
        DateOnly date,
        decimal amount)
    {
        return CreateTransaction(Guid.NewGuid(), date, amount);
    }

    private static Transaction CreateTransaction(
        Guid id,
        DateOnly date,
        decimal amount)
    {
        return new Transaction(
            id,
            amount,
            TransactionType.Credit,
            date,
            "Salary");
    }

    private static async Task SeedAsync(
        TransactionDbContext db,
        params Transaction[] transactions)
    {
        await db.Transactions.AddRangeAsync(transactions);
        await db.SaveChangesAsync();
    }
}
