using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace Flow.Transactions.Application.UseCases.Transactions.DeleteTransaction;
public sealed class DeleteTransactionService(
    ITransactionRepository repository,
    ITransactionDailyRecomputePublisher publisher,
    ILogger<DeleteTransactionService> logger)
    : IDeleteTransactionService
{
    public async Task<bool> ExecuteAsync(
        DeleteTransactionRequest request)
    {
        var tx = await repository.GetByIdAsync(request.Id);

        if (tx is null)
        {
            logger.LogWarning("Transaction {TransactionId} was not found for deletion", request.Id);
            return false;
        }

        var date = tx.Date;

        repository.Remove(tx);
        logger.LogInformation(
            "Deleted transaction {TransactionId} for {Date}",
            request.Id,
            date);

        await repository.SaveChangesAsync();

        await publisher.PublishAsync(new TransactionDailyRecomputeMessage(date));
        logger.LogInformation(
            "Published daily recompute request for deleted transaction {TransactionId} on {Date}",
            request.Id,
            date);

        return true;
    }
}
