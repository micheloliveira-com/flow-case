

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Flow.Reports.Domain.Entities;

public interface IGetTransactionDailyBalance
{
    Task<List<TransactionDailyBalance>> ExecuteAsync(
        GetTransactionDailyBalanceRequest request,
        CancellationToken cancellationToken = default);
}