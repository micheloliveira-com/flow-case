using Flow.Transactions.Domain.Entities;

namespace Flow.Transactions.Application.UseCases.Transactions.GetTransactions;
public interface IGetTransactionsService
{
    Task<List<Transaction>> ExecuteAsync(
        GetTransactionsRequest request);
}