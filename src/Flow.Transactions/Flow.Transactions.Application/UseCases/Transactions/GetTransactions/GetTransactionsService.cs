using Flow.Transactions.Application.Abstractions.Persistence;

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