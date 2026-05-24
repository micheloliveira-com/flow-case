using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.Abstractions.Messaging;

public sealed class DeleteTransactionService(
    ITransactionRepository repository,
    ITransactionDailyRecomputePublisher publisher)
    : IDeleteTransactionService
{
    public async Task<bool> ExecuteAsync(
        DeleteTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tx = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (tx is null)
            return false;

        var date = tx.Date;

        await repository.RemoveAsync(tx, cancellationToken);

        await publisher.PublishAsync(new TransactionDailyRecomputeMessage(date), cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
