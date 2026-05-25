using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Flow.Transactions.Application.UseCases.Transactions.CreateTransaction;
public sealed class CreateTransactionService(
    ITransactionRepository repository,
    ITransactionDailyRecomputePublisher publisher,
    ILogger<CreateTransactionService> logger)
    : ICreateTransactionService
{
    public async Task<Transaction> ExecuteAsync(
        CreateTransactionRequest request)
    {
        var transaction = CreateTransaction(request);

        await PersistAsync(transaction);

        LogTransactionCreated(transaction);

        await PublishDailyRecomputeAsync(transaction);

        return transaction;
    }

    private async Task PersistAsync(Transaction transaction)
    {
        await repository.AddAsync(transaction);
        await repository.SaveChangesAsync();
    }

    private async Task PublishDailyRecomputeAsync(Transaction transaction)
    {
        await publisher.PublishAsync(
            new TransactionDailyRecomputeMessage(transaction.Date));

        logger.LogInformation(
            "Published daily recompute request for transaction {TransactionId} on {Date}",
            transaction.Id,
            transaction.Date);
    }

    private static Transaction CreateTransaction(
        CreateTransactionRequest request)
    {
        return new Transaction(
            amount: request.Amount,
            type: request.Type,
            date: request.Date,
            description: request.Description);
    }

    private void LogTransactionCreated(Transaction transaction)
    {
        logger.LogInformation(
            "Created transaction {TransactionId} for {Date} with amount {Amount} and type {Type}",
            transaction.Id,
            transaction.Date,
            transaction.Amount,
            transaction.Type);
    }
}
