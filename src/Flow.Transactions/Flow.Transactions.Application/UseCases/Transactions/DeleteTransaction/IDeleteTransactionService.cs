namespace Flow.Transactions.Application.UseCases.Transactions.DeleteTransaction;
public interface IDeleteTransactionService
{
    Task<bool> ExecuteAsync(
        DeleteTransactionRequest request);
}
