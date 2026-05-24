

using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;

namespace Flow.Web.Blazor;

public class TransactionBalanceApiClient(HttpClient httpClient)
{
    public async Task<TransactionDailyBalance[]> GetTransactionDailyBalancesAsync(
        DateOnly? start,
        DateOnly? end,
        CancellationToken cancellationToken = default)
    {
        var url = "/transaction_daily_balance";

        var query = new Dictionary<string, string?>();

        if (start.HasValue)
            query["start"] = start.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (end.HasValue)
            query["end"] = end.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (query.Count > 0)
            url = QueryHelpers.AddQueryString(url, query);

        return await httpClient.GetFromJsonAsync<TransactionDailyBalance[]>(
                   url,
                   cancellationToken)
               ?? [];
    }
}

public record TransactionDailyBalance(
    Guid Id,
    decimal Balance,
    DateOnly Date,
    DateTime CreatedAtUtc);