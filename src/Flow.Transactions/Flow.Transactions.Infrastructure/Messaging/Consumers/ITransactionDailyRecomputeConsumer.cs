using Flow.Shared.Infrastructure.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Transactions.Infrastructure.Messaging.Consumers;

public interface ITransactionDailyRecomputeConsumer
{
    Task StartAsync(MessageHandler<TransactionDailyRecomputeMessage> handler, CancellationToken cancellationToken);
}