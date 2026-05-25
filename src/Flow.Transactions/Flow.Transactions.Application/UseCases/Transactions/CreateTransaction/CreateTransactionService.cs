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
        var tx = new Transaction(
            amount: request.Amount,
            type: request.Type,
            date: request.Date,
            description: request.Description
        );

        await repository.AddAsync(tx);
        logger.LogInformation(
            "Created transaction {TransactionId} for {Date} with amount {Amount} and type {Type}",
            tx.Id,
            tx.Date,
            tx.Amount,
            tx.Type);

        await repository.SaveChangesAsync();

        await publisher.PublishAsync(new TransactionDailyRecomputeMessage(tx.Date));
        logger.LogInformation(
            "Published daily recompute request for transaction {TransactionId} on {Date}",
            tx.Id,
            tx.Date);

        return tx;
    }
}
