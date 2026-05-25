using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;

namespace Flow.Transactions.Infrastructure.Messaging.Publishers;

public sealed class TransactionDailyRecomputePublisher(IMessagePublisher publisher) : ITransactionDailyRecomputePublisher
{
    private const string RoutingKey = "transaction-daily-recompute";

    public Task PublishAsync(
        TransactionDailyRecomputeMessage message)
    {
        return publisher.PublishAsync(
            RoutingKey,
            message);
    }
}
