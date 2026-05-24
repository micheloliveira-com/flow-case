

public interface IGetTransactionsService
{
    Task<List<Transaction>> ExecuteAsync(
        GetTransactionsRequest request);
}