using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Domain.Entities;
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
        var transaction = await repository.GetByIdAsync(request.Id);

        if (transaction is null)
        {
            LogTransactionNotFound(request.Id);
            return false;
        }

        await DeleteAsync(transaction);

        LogTransactionDeleted(request.Id, transaction.Date);

        await PublishDailyRecomputeAsync(
            request.Id,
            transaction.Date);

        return true;
    }

    private async Task DeleteAsync(Transaction transaction)
    {
        repository.Remove(transaction);
        await repository.SaveChangesAsync();
    }

    private async Task PublishDailyRecomputeAsync(
        Guid transactionId,
        DateOnly date)
    {
        await publisher.PublishAsync(
            new TransactionDailyRecomputeMessage(date));

        logger.LogInformation(
            "Published daily recompute request for transaction {TransactionId} on {Date}",
            transactionId,
            date);
    }

    private void LogTransactionNotFound(Guid transactionId)
    {
        logger.LogWarning(
            "Transaction {TransactionId} was not found for deletion",
            transactionId);
    }

    private void LogTransactionDeleted(
        Guid transactionId,
        DateOnly date)
    {
        logger.LogInformation(
            "Deleted transaction {TransactionId} for {Date}",
            transactionId,
            date);
    }
}
