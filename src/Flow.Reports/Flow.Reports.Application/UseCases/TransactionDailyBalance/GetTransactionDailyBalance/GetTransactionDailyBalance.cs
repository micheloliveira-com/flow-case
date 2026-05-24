

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Reports.Application.Abstractions.Persistence;
using Flow.Reports.Domain.Entities;

namespace Flow.Reports.Application.UseCases.TransactionDailyBalance.GetTransactionDailyBalance;

public sealed class GetTransactionDailyBalance(
    ITransactionDailyBalanceRepository repository)
    : IGetTransactionDailyBalance
{
    public async Task<List<Domain.Entities.TransactionDailyBalance>> ExecuteAsync(
        GetTransactionDailyBalanceRequest request)
    {
        return await repository.GetAsync(request.Start, request.End);
    }
}