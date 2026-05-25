using Microsoft.Extensions.Logging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Shared.Infrastructure.Abstractions.Messaging;

namespace Flow.Transactions.Infrastructure.Messaging.Consumers;

public sealed class TransactionDailyRecomputeConsumer(
    IMessageConsumer consumer,
    ILogger<TransactionDailyRecomputeConsumer> logger)
                        : ITransactionDailyRecomputeConsumer
{
    private const string QueueName = "transaction-daily-recompute";

    public async Task StartAsync(MessageHandler<TransactionDailyRecomputeMessage> handler,
                                CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Subscribing transaction daily recompute consumer to queue {QueueName}",
            QueueName);
        await consumer.SubscribeAsync<TransactionDailyRecomputeMessage>(
            QueueName,
            handler,
            cancellationToken);
    }
}
