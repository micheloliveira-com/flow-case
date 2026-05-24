using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Flow.Transactions.Infrastructure.Messaging.Messages;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;

namespace Flow.Transactions.Infrastructure.Messaging.Consumers;

public sealed class TransactionDailyRecomputeConsumer
                    (IExecuteTransactionDailyRecomputeService service, IMessageConsumer consumer)
                        : ITransactionDailyRecomputeConsumer
{
    private const string QueueName = "transaction-daily-recompute";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await consumer.SubscribeAsync<TransactionDailyRecomputeMessage>(
            QueueName,
            async message =>
            {
                await service.ExecuteAsync(message, cancellationToken);
            },
            cancellationToken);
    }
}