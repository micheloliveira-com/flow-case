public interface ICreateTransactionService
{
    Task<Transaction> ExecuteAsync(
        CreateTransactionRequest request);
}