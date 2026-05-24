

using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Infrastructure.Messaging.Messages;
using Flow.Transactions.Infrastructure.Messaging.Messages.TransactionDailyBalance;

public sealed class TransactionDailyBalancePublisher : ITransactionDailyBalancePublisher
{
    private const string RoutingKey = "transaction-daily-balance";

    private readonly IMessagePublisher _publisher;

    public TransactionDailyBalancePublisher(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public Task PublishAsync(
        TransactionDailyBalanceMessage message,
        CancellationToken cancellationToken = default)
    {
        return _publisher.PublishAsync(
            RoutingKey,
            message,
            cancellationToken);
    }
}