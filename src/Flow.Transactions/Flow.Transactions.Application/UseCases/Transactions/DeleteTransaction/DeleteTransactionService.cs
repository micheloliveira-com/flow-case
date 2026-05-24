using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.Abstractions.Messaging;

public sealed class DeleteTransactionService(
    ITransactionRepository repository,
    ITransactionDailyRecomputePublisher publisher)
    : IDeleteTransactionService
{
    public async Task<bool> ExecuteAsync(
        DeleteTransactionRequest request)
    {
        var tx = await repository.GetByIdAsync(request.Id);

        if (tx is null)
            return false;

        var date = tx.Date;

        await repository.RemoveAsync(tx);

        await repository.SaveChangesAsync();
        
        await publisher.PublishAsync(new TransactionDailyRecomputeMessage(date));

        
        return true;
    }
}
