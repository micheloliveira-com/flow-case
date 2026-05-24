

using System.Threading;
using System.Threading.Tasks;
using Flow.Shared.Application.Abstractions.Messaging;

namespace Flow.Reports.Application.UseCases.DailyBalance.ExecuteTransactionDailyBalance;

public interface IExecuteTransactionDailyBalanceService
{
    Task ExecuteAsync(
        TransactionDailyBalanceMessage message);
}