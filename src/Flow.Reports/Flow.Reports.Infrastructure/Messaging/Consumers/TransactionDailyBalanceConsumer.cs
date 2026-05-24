using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Shared.Infrastructure.Abstractions.Messaging;

namespace Flow.Reports.Infrastructure.Messaging.Consumers;
public sealed class TransactionDailyBalanceConsumer(IMessageConsumer consumer)
                        : ITransactionDailyBalanceConsumer
{
    private const string QueueName = "transaction-daily-balance";

    public async Task StartAsync(MessageHandler<TransactionDailyBalanceMessage> handler,
                                CancellationToken cancellationToken)
    {
        await consumer.SubscribeAsync<TransactionDailyBalanceMessage>(
            QueueName,
            handler,
            cancellationToken);
    }
}