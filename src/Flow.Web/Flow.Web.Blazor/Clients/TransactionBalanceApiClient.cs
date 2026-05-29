using System.Globalization;
using System.Net.Http.Json;
using Flow.Web.Blazor.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Flow.Web.Blazor.Clients;

public class TransactionBalanceApiClient(
    HttpClient httpClient,
    ILogger<TransactionBalanceApiClient> logger)
{
    public async Task<TransactionDailyBalanceModel[]> GetTransactionDailyBalancesAsync(
        DateOnly? start,
        DateOnly? end)
    {
        var url = BuildTransactionDailyBalancesUrl(
            start,
            end);

        var response = await SendGetRequestAsync(url);

        await EnsureSuccessStatusCodeAsync(response);

        var balances = await ReadBalancesAsync(response);

        LogBalancesRetrieved(balances.Length);

        return balances;
    }

    private static string BuildTransactionDailyBalancesUrl(
        DateOnly? start,
        DateOnly? end)
    {
        var url = "/transaction_daily_balance";

        var query = CreateQuery(start, end);

        return query.Count > 0
            ? QueryHelpers.AddQueryString(url, query)
            : url;
    }

    private static Dictionary<string, string?> CreateQuery(
        DateOnly? start,
        DateOnly? end)
    {
        var query = new Dictionary<string, string?>();

        AddStartDateQuery(query, start);

        AddEndDateQuery(query, end);

        return query;
    }

    private static void AddStartDateQuery(
        IDictionary<string, string?> query,
        DateOnly? start)
    {
        if (start.HasValue)
        {
            query["start"] = start.Value.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);
        }
    }

    private static void AddEndDateQuery(
        IDictionary<string, string?> query,
        DateOnly? end)
    {
        if (end.HasValue)
        {
            query["end"] = end.Value.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);
        }
    }

    private async Task<HttpResponseMessage> SendGetRequestAsync(string url)
    {
        return await httpClient.GetAsync(url);
    }

    private async Task EnsureSuccessStatusCodeAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await ApiErrorReader.ReadMessageAsync(response);

        LogApiFailure(
            response,
            message);

        throw new ApiClientException(message);
    }

    private void LogApiFailure(
        HttpResponseMessage response,
        string message)
    {
        logger.LogWarning(
            "Failed to retrieve transaction daily balances. Status code: {StatusCode}. Message: {Message}",
            response.StatusCode,
            message);
    }

    private static async Task<TransactionDailyBalanceModel[]> ReadBalancesAsync(
        HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<TransactionDailyBalanceModel[]>()
            ?? [];
    }

    private void LogBalancesRetrieved(int balanceCount)
    {
        logger.LogInformation(
            "Retrieved {BalanceCount} transaction daily balances from reports API",
            balanceCount);
    }
}
