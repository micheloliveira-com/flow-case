

using Flow.Transactions.Infrastructure.Messaging.Messages;

namespace Flow.Transactions.Infrastructure.Messaging.Messages.TransactionDailyBalance;

public interface ITransactionDailyBalancePublisher
{
    Task PublishAsync(
        TransactionDailyBalanceMessage message,
        CancellationToken cancellationToken = default);
}