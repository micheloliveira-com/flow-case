using System.Threading;
using System.Threading.Tasks;
using Flow.Shared.Application.Abstractions.Messaging;
using Flow.Shared.Infrastructure.Abstractions.Messaging;

namespace Flow.Reports.Infrastructure.Messaging.Consumers;
public interface ITransactionDailyBalanceConsumer
{
    Task StartAsync(
        MessageHandler<TransactionDailyBalanceMessage> handler,
        CancellationToken cancellationToken);
}