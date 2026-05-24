using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;

public sealed class CreateTransactionService(
    ITransactionRepository repository,
    ITransactionDailyRecomputePublisher publisher)
    : ICreateTransactionService
{
    public async Task<Transaction> ExecuteAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tx = new Transaction(
            amount: request.Amount,
            type: request.Type,
            date: request.Date,
            description: request.Description
        );

        await repository.AddAsync(tx, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);
        
        await publisher.PublishAsync(new TransactionDailyRecomputeMessage(tx.Date), cancellationToken);

        
        return tx;
    }
}