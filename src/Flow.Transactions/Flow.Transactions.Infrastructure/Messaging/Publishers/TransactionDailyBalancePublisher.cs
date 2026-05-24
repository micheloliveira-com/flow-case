

using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyBalance;

namespace Flow.Transactions.Infrastructure.Messaging.Publishers;

public sealed class TransactionDailyBalancePublisher : ITransactionDailyBalancePublisher
{
    private const string RoutingKey = "transaction-daily-balance";

    private readonly IMessagePublisher _publisher;

    public TransactionDailyBalancePublisher(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public Task PublishAsync(
        TransactionDailyBalanceMessage message)
    {
        return _publisher.PublishAsync(
            RoutingKey,
            message);
    }
}