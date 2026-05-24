using System.Threading;
using System.Threading.Tasks;

namespace Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;

public interface IExecuteTransactionDailyRecomputeService
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}