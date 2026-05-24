public interface IDeleteTransactionService
{
    Task<bool> ExecuteAsync(
        DeleteTransactionRequest request,
        CancellationToken cancellationToken = default);
}
