

using System.Threading;
using System.Threading.Tasks;
using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging;

namespace Flow.Transactions.Infrastructure.Messaging.Consumers;

public interface ITransactionDailyBalanceConsumer
{
    Task StartAsync(
        MessageHandler<TransactionDailyBalanceMessage> handler,
        CancellationToken cancellationToken);
}