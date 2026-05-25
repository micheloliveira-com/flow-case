

using Flow.Shared.Application.Abstractions.Messaging;

namespace Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyBalance;

public interface ITransactionDailyBalancePublisher
{
    Task PublishAsync(
        TransactionDailyBalanceMessage message);
}