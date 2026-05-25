using Flow.Transactions.Domain.Entities;

namespace Flow.Transactions.Application.UseCases.Transactions.CreateTransaction;
public interface ICreateTransactionService
{
    Task<Transaction> ExecuteAsync(
        CreateTransactionRequest request);
}