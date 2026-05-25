using Flow.Reports.Domain.Entities;
using Flow.Reports.Infrastructure.Persistence;
using Flow.Reports.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Flow.Reports.Tests.Infrastructure;

public sealed class TransactionDailyBalanceRepositoryTests
{
    [Fact]
    public async Task GetAsync_WhenNoPeriodIsProvided_ShouldReturnNonZeroBalancesOrderedByDateDescending()
    {
        // Arrange
        await using var db = CreateDbContext();
        await SeedAsync(
            db,
            CreateBalance(new DateOnly(2026, 5, 22), 100m),
            CreateBalance(new DateOnly(2026, 5, 24), 300m),
            CreateBalance(new DateOnly(2026, 5, 23), 0m));

        var repository = new TransactionDailyBalanceRepository(db);

        // Act
        var actual = await repository.GetAsync(start: null, end: null);

        // Assert
        Assert.Collection(
            actual,
            item => Assert.Equal(new DateOnly(2026, 5, 24), item.Date),
            item => Assert.Equal(new DateOnly(2026, 5, 22), item.Date));
    }

    [Fact]
    public async Task GetAsync_WhenPeriodIsProvided_ShouldApplyInclusiveDateFilters()
    {
        // Arrange
        await using var db = CreateDbContext();
        await SeedAsync(
            db,
            CreateBalance(new DateOnly(2026, 5, 20), 100m),
            CreateBalance(new DateOnly(2026, 5, 21), 200m),
            CreateBalance(new DateOnly(2026, 5, 23), 300m),
            CreateBalance(new DateOnly(2026, 5, 24), 400m));

        var repository = new TransactionDailyBalanceRepository(db);

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
    public async Task GetByDateAsync_WhenDateExists_ShouldReturnTrackedEntity()
    {
        // Arrange
        await using var db = CreateDbContext();
        var expected = CreateBalance(new DateOnly(2026, 5, 24), 300m);
        await SeedAsync(db, expected);

        var repository = new TransactionDailyBalanceRepository(db);

        // Act
        var actual = await repository.GetByDateAsync(expected.Date);

        // Assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task AddAsyncAndSaveChangesAsync_ShouldPersistEntity()
    {
        // Arrange
        await using var db = CreateDbContext();
        var repository = new TransactionDailyBalanceRepository(db);
        var entity = CreateBalance(new DateOnly(2026, 5, 24), 300m);

        // Act
        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await db.TransactionDailyBalance.SingleAsync();
        Assert.Equal(entity.Date, persisted.Date);
        Assert.Equal(entity.Balance, persisted.Balance);
        Assert.Equal(entity.ProcessedAt, persisted.ProcessedAt);
    }

    private static ReportsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ReportsDbContext(options);
    }

    private static TransactionDailyBalance CreateBalance(
        DateOnly date,
        decimal balance)
    {
        return new TransactionDailyBalance(
            date,
            balance,
            new DateTime(2026, 5, 24, 10, 0, 0, DateTimeKind.Utc));
    }

    private static async Task SeedAsync(
        ReportsDbContext db,
        params TransactionDailyBalance[] balances)
    {
        await db.TransactionDailyBalance.AddRangeAsync(balances);
        await db.SaveChangesAsync();
    }
}
