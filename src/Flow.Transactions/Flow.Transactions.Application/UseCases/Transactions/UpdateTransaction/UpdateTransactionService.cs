using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Domain.Entities;

namespace Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;

public sealed class UpdateTransactionService(
    ITransactionRepository repository,
    ITransactionDailyRecomputePublisher publisher)
    : IUpdateTransactionService
{
    public async Task<Transaction?> ExecuteAsync(
        Guid id,
        UpdateTransactionRequest request)
    {
        var tx = await repository.GetByIdAsync(id);

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

        await repository.UpdateAsync(updated);

        var affectedDates = new HashSet<DateOnly>
        {
            oldDate,
            request.Date
        };

        await repository.SaveChangesAsync();
        
        foreach (var date in affectedDates)
        {
            await publisher.PublishAsync(new TransactionDailyRecomputeMessage(date));
        }

        
        return updated;
    }
}