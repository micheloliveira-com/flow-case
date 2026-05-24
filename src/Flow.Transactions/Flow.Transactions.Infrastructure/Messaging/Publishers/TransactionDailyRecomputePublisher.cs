using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;

namespace Flow.Transactions.Infrastructure.Messaging.Publishers;

public sealed class TransactionDailyRecomputePublisher : ITransactionDailyRecomputePublisher
{
    private const string RoutingKey = "transaction-daily-recompute";

    private readonly IMessagePublisher _publisher;

    public TransactionDailyRecomputePublisher(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public Task PublishAsync(
        TransactionDailyRecomputeMessage message)
    {
        return _publisher.PublishAsync(
            RoutingKey,
            message);
    }
}
