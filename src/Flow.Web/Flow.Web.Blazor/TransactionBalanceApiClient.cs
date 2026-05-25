

using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Flow.Web.Blazor;

public class TransactionBalanceApiClient(
    HttpClient httpClient,
    ILogger<TransactionBalanceApiClient> logger)
{
    public async Task<TransactionDailyBalance[]> GetTransactionDailyBalancesAsync(
        DateOnly? start,
        DateOnly? end)
    {
        var url = "/transaction_daily_balance";

        var query = new Dictionary<string, string?>();

        if (start.HasValue)
            query["start"] = start.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (end.HasValue)
            query["end"] = end.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (query.Count > 0)
            url = QueryHelpers.AddQueryString(url, query);

        var balances = await httpClient.GetFromJsonAsync<TransactionDailyBalance[]>(
            url) ?? [];
        logger.LogInformation(
            "Retrieved {BalanceCount} transaction daily balances from reports API",
            balances.Length);

        return balances;
    }
}

public record TransactionDailyBalance(
    Guid Id,
    decimal Balance,
    DateOnly Date,
    DateTime CreatedAtUtc);
