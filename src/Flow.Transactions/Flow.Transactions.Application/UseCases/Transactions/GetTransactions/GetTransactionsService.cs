using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Flow.Transactions.Application.UseCases.Transactions.GetTransactions;
public sealed class GetTransactionsService(
    ITransactionRepository repository,
    ILogger<GetTransactionsService> logger)
    : IGetTransactionsService
{
    public async Task<List<Transaction>> ExecuteAsync(
        GetTransactionsRequest request)
    {
        var transactions = await repository.GetAsync(request.Start, request.End);
        logger.LogInformation(
            "Retrieved {TransactionCount} transactions from {StartDate} to {EndDate}",
            transactions.Count,
            request.Start,
            request.End);

        return transactions;
    }
}
