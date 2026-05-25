using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyBalance;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
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

        logger.LogInformation(
            "Recomputing daily balance for {Date}",
            date);

        var balance = await repository.GetDailyBalanceAsync(date);

        await publisher.PublishAsync(
            new TransactionDailyBalanceMessage(date, balance, DateTime.UtcNow));
        logger.LogInformation(
            "Published daily balance for {Date} with balance {Balance}",
            date,
            balance);
    }
}
