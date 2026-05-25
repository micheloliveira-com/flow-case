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
        var transactions = await GetTransactionsAsync(request);

        LogTransactionsRetrieved(request, transactions.Count);

        return transactions;
    }

    private async Task<List<Transaction>> GetTransactionsAsync(
        GetTransactionsRequest request)
    {
        return await repository.GetAsync(
            request.Start,
            request.End);
    }

    private void LogTransactionsRetrieved(
        GetTransactionsRequest request,
        int transactionCount)
    {
        logger.LogInformation(
            "Retrieved {TransactionCount} transactions from {StartDate} to {EndDate}",
            transactionCount,
            request.Start,
            request.End);
    }
}
