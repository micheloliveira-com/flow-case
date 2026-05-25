using Flow.Reports.Domain.Entities;
using Microsoft.Extensions.Logging;
using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Reports.Application.Abstractions.Persistence;

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

        if (ShouldIgnore(message, current))
        {
            LogOutdatedMessageIgnored(message, current!);
            return;
        }

        await PersistTransactionDailyBalanceAsync(message, current);

        await repository.SaveChangesAsync();
    }

    private async Task PersistTransactionDailyBalanceAsync(
        TransactionDailyBalanceMessage message,
        Domain.Entities.TransactionDailyBalance? current)
    {
        if (current is null)
        {
            await CreateTransactionDailyBalanceAsync(message);
            return;
        }

        UpdateTransactionDailyBalance(current, message);

        LogTransactionDailyBalanceUpdated(message);
    }

    private async Task CreateTransactionDailyBalanceAsync(
        TransactionDailyBalanceMessage message)
    {
        var transactionDailyBalance = CreateTransactionDailyBalance(message);

        await repository.AddAsync(transactionDailyBalance);

        LogTransactionDailyBalanceCreated(message);
    }

    private static bool ShouldIgnore(
        TransactionDailyBalanceMessage message,
        Domain.Entities.TransactionDailyBalance? current)
    {
        return current is not null &&
               current.ProcessedAt >= message.ProcessedAt;
    }

    private static Domain.Entities.TransactionDailyBalance CreateTransactionDailyBalance(
        TransactionDailyBalanceMessage message)
    {
        return new Domain.Entities.TransactionDailyBalance(
            message.Date,
            message.Balance,
            message.ProcessedAt);
    }

    private static void UpdateTransactionDailyBalance(
        Domain.Entities.TransactionDailyBalance current,
        TransactionDailyBalanceMessage message)
    {
        current.Apply(
            message.Balance,
            message.ProcessedAt);
    }

    private void LogOutdatedMessageIgnored(
        TransactionDailyBalanceMessage message,
        Domain.Entities.TransactionDailyBalance current)
    {
        logger.LogInformation(
            "Ignoring outdated transaction daily balance for {Date}. Current ProcessedAt: {CurrentProcessedAt}. Incoming ProcessedAt: {IncomingProcessedAt}",
            message.Date,
            current.ProcessedAt,
            message.ProcessedAt);
    }

    private void LogTransactionDailyBalanceCreated(
        TransactionDailyBalanceMessage message)
    {
        logger.LogInformation(
            "Created transaction daily balance for {Date} with balance {Balance} processed at {ProcessedAt}",
            message.Date,
            message.Balance,
            message.ProcessedAt);
    }

    private void LogTransactionDailyBalanceUpdated(
        TransactionDailyBalanceMessage message)
    {
        logger.LogInformation(
            "Updated transaction daily balance for {Date} with balance {Balance} processed at {ProcessedAt}",
            message.Date,
            message.Balance,
            message.ProcessedAt);
    }
}
