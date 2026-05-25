using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyBalance;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Domain.Entities.Enums;
using Microsoft.Extensions.Logging;

namespace Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;

public sealed class ExecuteTransactionDailyRecomputeService(
    ITransactionRepository repository,
    ITransactionDailyBalancePublisher publisher,
    ILogger<ExecuteTransactionDailyRecomputeService> logger) : IExecuteTransactionDailyRecomputeService
{

    public async Task ExecuteAsync(TransactionDailyRecomputeMessage message)
    {
        var date = message.Date;

        var transactions = await repository.GetByDateAsync(date);
        logger.LogInformation(
            "Recomputing daily balance for {Date} from {TransactionCount} transactions",
            date,
            transactions.Count);

        var balance = transactions.Sum(x =>
            x.Type == TransactionType.Credit
                ? x.Amount
                : -x.Amount);

        await publisher.PublishAsync(
            new TransactionDailyBalanceMessage(date, balance, DateTime.UtcNow));
        logger.LogInformation(
            "Published daily balance for {Date} with balance {Balance}",
            date,
            balance);
    }
}
