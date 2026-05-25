

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Flow.Reports.Domain.Entities;

namespace Flow.Reports.Application.UseCases.TransactionDailyBalance.GetTransactionDailyBalance;

public interface IGetTransactionDailyBalance
{
    Task<List<Domain.Entities.TransactionDailyBalance>> ExecuteAsync(
        GetTransactionDailyBalanceRequest request);
}