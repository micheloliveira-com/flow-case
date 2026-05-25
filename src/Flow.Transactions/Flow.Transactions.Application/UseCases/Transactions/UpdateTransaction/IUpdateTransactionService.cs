

using Flow.Transactions.Domain.Entities;

namespace Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;

public interface IUpdateTransactionService
{
    Task<Transaction?> ExecuteAsync(
        Guid id,
        UpdateTransactionRequest request);
}