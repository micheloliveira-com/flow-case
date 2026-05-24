using Flow.Transactions.Domain.Entities;

namespace Flow.Transactions.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);

    Task<List<Transaction>> GetAsync(
        DateOnly? start,
        DateOnly? end);

    Task<List<Transaction>> GetByDateAsync(DateOnly date);

    Task AddAsync(Transaction transaction);

    Task UpdateAsync(Transaction transaction);

    Task RemoveAsync(Transaction transaction);

    Task SaveChangesAsync();
}