

public interface IUpdateTransactionService
{
    Task<Transaction?> ExecuteAsync(
        Guid id,
        UpdateTransactionRequest request);
}