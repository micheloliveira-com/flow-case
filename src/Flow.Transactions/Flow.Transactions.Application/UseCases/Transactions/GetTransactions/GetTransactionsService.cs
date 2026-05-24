using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Domain.Entities;

namespace Flow.Transactions.Application.UseCases.Transactions.GetTransactions;
public sealed class GetTransactionsService(
    ITransactionRepository repository)
    : IGetTransactionsService
{
    public async Task<List<Transaction>> ExecuteAsync(
        GetTransactionsRequest request)
    {
        return await repository.GetAsync(request.Start, request.End);
    }
}