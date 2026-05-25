

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Reports.Application.Abstractions.Persistence;
using Flow.Reports.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Flow.Reports.Application.UseCases.TransactionDailyBalance.GetTransactionDailyBalance;

public sealed class GetTransactionDailyBalance(
    ITransactionDailyBalanceRepository repository,
    ILogger<GetTransactionDailyBalance> logger)
    : IGetTransactionDailyBalance
{
    public async Task<List<Domain.Entities.TransactionDailyBalance>> ExecuteAsync(
        GetTransactionDailyBalanceRequest request)
    {
        var balances = await repository.GetAsync(request.Start, request.End);
        logger.LogInformation(
            "Retrieved {BalanceCount} transaction daily balances from {StartDate} to {EndDate}",
            balances.Count,
            request.Start,
            request.End);

        return balances;
    }
}
