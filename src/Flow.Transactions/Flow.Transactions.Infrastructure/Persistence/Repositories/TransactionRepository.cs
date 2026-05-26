using Flow.Transactions.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Infrastructure.Persistence;
using Flow.Transactions.Domain.Entities;
using Flow.Transactions.Domain.Entities.Enums;

namespace Flow.Transactions.Infrastructure.Persistence.Repositories;
public sealed class TransactionRepository(
    TransactionDbContext db) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await db.Transactions.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Transaction>> GetAsync(DateOnly? start, DateOnly? end)
    {
        var query = db.Transactions.AsNoTracking();

        if (start.HasValue)
            query = query.Where(x => x.Date >= start.Value);

        if (end.HasValue)
            query = query.Where(x => x.Date <= end.Value);

        return await query.OrderByDescending(x => x.Date).ToListAsync();
    }

    public async Task<decimal> GetDailyBalanceAsync(DateOnly date)
    {
        return await db.Transactions
            .AsNoTracking()
            .Where(x => x.Date == date)
            .SumAsync(Transaction.SignedAmountExpression);
    }

    public async Task AddAsync(Transaction transaction)
    {
        await db.Transactions.AddAsync(transaction);
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        await db.Transactions
            .Where(x => x.Id == transaction.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Amount, transaction.Amount)
                .SetProperty(x => x.Type, transaction.Type)
                .SetProperty(x => x.Date, transaction.Date)
                .SetProperty(x => x.Description, transaction.Description));
    }

    public void Remove(Transaction transaction)
    {
        db.Transactions.Remove(transaction);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }
}