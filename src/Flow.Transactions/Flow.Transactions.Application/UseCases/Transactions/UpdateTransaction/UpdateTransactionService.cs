using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;

public sealed class UpdateTransactionService(
    ITransactionRepository repository,
    ITransactionDailyRecomputePublisher publisher,
    ILogger<UpdateTransactionService> logger)
    : IUpdateTransactionService
{
    public async Task<Transaction?> ExecuteAsync(
        Guid id,
        UpdateTransactionRequest request)
    {
        var existing = await repository.GetByIdAsync(id);

        if (existing is null)
        {
            LogTransactionNotFound(id);
            return null;
        }

        var updated = CreateUpdatedTransaction(id, request);

        await PersistAsync(updated);

        LogTransactionUpdated(id, existing.Date, request.Date);

        await PublishDailyRecomputeAsync(
            id,
            existing.Date,
            request.Date);

        return updated;
    }

    private async Task PersistAsync(Transaction transaction)
    {
        await repository.UpdateAsync(transaction);
        await repository.SaveChangesAsync();
    }

    private async Task PublishDailyRecomputeAsync(
        Guid transactionId,
        DateOnly oldDate,
        DateOnly newDate)
    {
        foreach (var date in GetAffectedDates(oldDate, newDate))
        {
            await publisher.PublishAsync(
                new TransactionDailyRecomputeMessage(date));

            logger.LogInformation(
                "Published daily recompute request for transaction {TransactionId} on {Date}",
                transactionId,
                date);
        }
    }

    private static Transaction CreateUpdatedTransaction(
        Guid id,
        UpdateTransactionRequest request)
    {
        return new Transaction(
            id: id,
            amount: request.Amount,
            type: request.Type,
            date: request.Date,
            description: request.Description);
    }

    private static IEnumerable<DateOnly> GetAffectedDates(
        DateOnly oldDate,
        DateOnly newDate)
    {
        return new[] { oldDate, newDate }.Distinct();
    }

    private void LogTransactionNotFound(Guid id)
    {
        logger.LogWarning(
            "Transaction {TransactionId} was not found for update",
            id);
    }

    private void LogTransactionUpdated(
        Guid id,
        DateOnly oldDate,
        DateOnly newDate)
    {
        logger.LogInformation(
            "Updated transaction {TransactionId}. Old date: {OldDate}. New date: {NewDate}",
            id,
            oldDate,
            newDate);
    }
}
