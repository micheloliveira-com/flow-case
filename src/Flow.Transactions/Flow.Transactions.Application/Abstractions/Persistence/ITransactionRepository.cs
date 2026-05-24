namespace Flow.Transactions.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<List<Transaction>> GetAsync(
        DateOnly? start,
        DateOnly? end,
        CancellationToken ct);

    Task AddAsync(Transaction transaction, CancellationToken ct);

    Task UpdateAsync(Transaction transaction, CancellationToken ct);

    Task RemoveAsync(Transaction transaction, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}