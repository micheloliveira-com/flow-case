using Flow.Transactions.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Flow.Transactions.Application.Abstractions.Persistence;
public sealed class TransactionRepository(
    TransactionDbContext db) : ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Transactions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Transaction>> GetAsync(DateOnly? start, DateOnly? end, CancellationToken ct)
    {
        var query = db.Transactions.AsNoTracking();

        if (start.HasValue)
            query = query.Where(x => x.Date >= start.Value);

        if (end.HasValue)
            query = query.Where(x => x.Date <= end.Value);

        return await query.OrderByDescending(x => x.Date).ToListAsync(ct);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken ct)
    {
        await db.Transactions.AddAsync(transaction, ct);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken ct)
    {
        await db.Transactions
            .Where(x => x.Id == transaction.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Amount, transaction.Amount)
                .SetProperty(x => x.Type, transaction.Type)
                .SetProperty(x => x.Date, transaction.Date)
                .SetProperty(x => x.Description, transaction.Description),
                ct);
    }

    public Task RemoveAsync(Transaction transaction, CancellationToken ct)
    {
        db.Transactions.Remove(transaction);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
        => await db.SaveChangesAsync(ct);
}