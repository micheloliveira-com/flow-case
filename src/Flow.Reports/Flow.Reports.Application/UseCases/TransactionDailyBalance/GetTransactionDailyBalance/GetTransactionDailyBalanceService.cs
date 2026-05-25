using System.Threading.Tasks;
using Flow.Reports.Application.Abstractions.Persistence;
using Microsoft.Extensions.Logging;

namespace Flow.Reports.Application.UseCases.TransactionDailyBalance.GetTransactionDailyBalance;

public sealed class GetTransactionDailyBalanceService(
    ITransactionDailyBalanceRepository repository,
    ILogger<GetTransactionDailyBalanceService> logger)
    : IGetTransactionDailyBalanceService
{
    public async Task<List<Domain.Entities.TransactionDailyBalance>> ExecuteAsync(
        GetTransactionDailyBalanceRequest request)
    {
        var balances = await GetBalancesAsync(request);

        LogBalancesRetrieved(request, balances.Count);

        return balances;
    }

    private async Task<List<Domain.Entities.TransactionDailyBalance>> GetBalancesAsync(
        GetTransactionDailyBalanceRequest request)
    {
        return await repository.GetAsync(
            request.Start,
            request.End);
    }

    private void LogBalancesRetrieved(
        GetTransactionDailyBalanceRequest request,
        int balanceCount)
    {
        logger.LogInformation(
            "Retrieved {BalanceCount} transaction daily balances from {StartDate} to {EndDate}",
            balanceCount,
            request.Start,
            request.End);
    }
}
