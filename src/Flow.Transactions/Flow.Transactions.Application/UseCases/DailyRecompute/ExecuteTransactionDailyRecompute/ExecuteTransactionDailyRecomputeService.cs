using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyBalance;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Microsoft.Extensions.Logging;

namespace Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;

public sealed class ExecuteTransactionDailyRecomputeService(
    ITransactionRepository repository,
    ITransactionDailyBalancePublisher publisher,
    ILogger<ExecuteTransactionDailyRecomputeService> logger)
    : IExecuteTransactionDailyRecomputeService
{
    public async Task ExecuteAsync(
        TransactionDailyRecomputeMessage message)
    {
        var date = GetMessageDate(message);

        LogDailyBalanceRecomputing(date);

        var balance = await GetDailyBalanceAsync(date);

        await PublishDailyBalanceAsync(
            date,
            balance);
    }

    private static DateOnly GetMessageDate(
        TransactionDailyRecomputeMessage message)
    {
        return message.Date;
    }

    private async Task<decimal> GetDailyBalanceAsync(DateOnly date)
    {
        return await repository.GetDailyBalanceAsync(date);
    }

    private async Task PublishDailyBalanceAsync(
        DateOnly date,
        decimal balance)
    {
        var dailyBalanceMessage = CreateDailyBalanceMessage(
            date,
            balance);

        await publisher.PublishAsync(dailyBalanceMessage);

        LogDailyBalancePublished(
            date,
            balance);
    }

    private static TransactionDailyBalanceMessage CreateDailyBalanceMessage(
        DateOnly date,
        decimal balance)
    {
        return new TransactionDailyBalanceMessage(
            date,
            balance,
            DateTime.UtcNow);
    }

    private void LogDailyBalanceRecomputing(DateOnly date)
    {
        logger.LogInformation(
            "Recomputing daily balance for {Date}",
            date);
    }

    private void LogDailyBalancePublished(
        DateOnly date,
        decimal balance)
    {
        logger.LogInformation(
            "Published daily balance for {Date} with balance {Balance}",
            date,
            balance);
    }
}
