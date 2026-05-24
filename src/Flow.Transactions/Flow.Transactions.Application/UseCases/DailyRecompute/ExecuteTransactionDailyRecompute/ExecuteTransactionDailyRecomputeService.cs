using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;
using Flow.Transactions.Infrastructure.Messaging.Messages.TransactionDailyBalance;

public sealed class ExecuteTransactionDailyRecomputeService(
        ITransactionRepository repository,
        ITransactionDailyBalancePublisher publisher) : IExecuteTransactionDailyRecomputeService
{

    public async Task ExecuteAsync(TransactionDailyRecomputeMessage message, CancellationToken cancellationToken = default)
    {
        var date = message.Date;

        var transactions = await repository.GetAsync(date, date, cancellationToken);

        var balance = transactions.Sum(x =>
            x.Type == TransactionType.Credit
                ? x.Amount
                : -x.Amount);

        await publisher.PublishAsync(
            new TransactionDailyBalanceMessage(date, balance, DateTime.UtcNow),
            cancellationToken);
    }
}