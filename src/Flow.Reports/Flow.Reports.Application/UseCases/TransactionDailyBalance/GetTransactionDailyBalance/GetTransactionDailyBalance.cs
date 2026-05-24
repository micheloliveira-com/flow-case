

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Reports.Domain.Entities;

public sealed class GetTransactionDailyBalance(
    ITransactionDailyBalanceRepository repository)
    : IGetTransactionDailyBalance
{
    public async Task<List<TransactionDailyBalance>> ExecuteAsync(
        GetTransactionDailyBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        return await repository.GetAsync(request.Start, request.End, cancellationToken);
    }
}