using Flow.Reports.Domain.Entities;
using Microsoft.Extensions.Logging;
using Flow.Shared.Application.Abstractions.Messaging;

namespace Flow.Reports.Application.UseCases.DailyBalance.ExecuteTransactionDailyBalance;

public sealed class ExecuteTransactionDailyBalanceService(
    ITransactionDailyBalanceRepository repository,
    ILogger<ExecuteTransactionDailyBalanceService> logger)
    : IExecuteTransactionDailyBalanceService
{
    public async Task ExecuteAsync(
        TransactionDailyBalanceMessage message)
    {
        var current = await repository.GetByDateAsync(message.Date);

        if (current is not null &&
            current.ProcessedAt >= message.ProcessedAt)
        {
            logger.LogInformation(
                "Ignoring outdated transaction daily balance for {Date}. Current ProcessedAt: {CurrentProcessedAt}. Incoming ProcessedAt: {IncomingProcessedAt}",
                message.Date,
                current.ProcessedAt,
                message.ProcessedAt);
            return;
        }

        if (current is null)
        {
            current = new TransactionDailyBalance(
                message.Date,
                message.Balance,
                message.ProcessedAt);

            await repository.AddAsync(current);
        }
        else
        {
            current.Apply(message.Balance, message.ProcessedAt);
        }

        await repository.SaveAsync();
    }
}