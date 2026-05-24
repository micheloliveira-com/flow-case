

using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyBalance;

namespace Flow.Transactions.Infrastructure.Messaging.Publishers;

public sealed class TransactionDailyBalancePublisher(IMessagePublisher publisher) : ITransactionDailyBalancePublisher
{
    private const string RoutingKey = "transaction-daily-balance";

    public Task PublishAsync(
        TransactionDailyBalanceMessage message)
    {
        return publisher.PublishAsync(
            RoutingKey,
            message);
    }
}