using System.Threading;
using System.Threading.Tasks;

namespace Flow.Transactions.Infrastructure.Messaging.Consumers;

public interface ITransactionDailyRecomputeConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
}