using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;

namespace Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;

public sealed class UpdateTransactionService(
    ITransactionRepository repository,
    ITransactionDailyRecomputePublisher publisher)
    : IUpdateTransactionService
{
    public async Task<Transaction?> ExecuteAsync(
        Guid id,
        UpdateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tx = await repository.GetByIdAsync(id, cancellationToken);

        if (tx is null)
            return null;

        var oldDate = tx.Date;

        var updated = new Transaction(
            id: id,
            amount: request.Amount,
            type: request.Type,
            date: request.Date,
            description: request.Description
        );

        await repository.UpdateAsync(updated, cancellationToken);

        var affectedDates = new HashSet<DateOnly>
        {
            oldDate,
            request.Date
        };

        await repository.SaveChangesAsync(cancellationToken);
        
        foreach (var date in affectedDates)
        {
            await publisher.PublishAsync(new TransactionDailyRecomputeMessage(date), cancellationToken);
        }

        
        return updated;
    }
}