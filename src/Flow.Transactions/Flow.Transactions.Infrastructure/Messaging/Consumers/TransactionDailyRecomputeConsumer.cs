using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;
using Flow.Transactions.Domain.Entities;
using Flow.Shared.Infrastructure.Abstractions.Messaging;

namespace Flow.Transactions.Infrastructure.Messaging.Consumers;

public sealed class TransactionDailyRecomputeConsumer(IMessageConsumer consumer)
                        : ITransactionDailyRecomputeConsumer
{
    private const string QueueName = "transaction-daily-recompute";

    public async Task StartAsync(MessageHandler<TransactionDailyRecomputeMessage> handler,
                                CancellationToken cancellationToken)
    {
        await consumer.SubscribeAsync<TransactionDailyRecomputeMessage>(
            QueueName,
            handler,
            cancellationToken);
    }
}