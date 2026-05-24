using System.Threading;
using System.Threading.Tasks;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;

namespace Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;

public interface IExecuteTransactionDailyRecomputeService
{
    Task ExecuteAsync(TransactionDailyRecomputeMessage message);
}