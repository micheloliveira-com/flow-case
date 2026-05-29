using Flow.Web.Blazor.Clients.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Flow.Web.Blazor.Clients;

public class TransactionApiClient(
    HttpClient httpClient,
    ILogger<TransactionApiClient> logger)
{
    public async Task<Transaction[]> GetTransactionsAsync(
        DateOnly? start,
        DateOnly? end)
    {
        var url = BuildTransactionsUrl(
            start,
            end);

        var transactions = await GetTransactionsFromApiAsync(url);

        LogTransactionsRetrieved(transactions.Length);

        return transactions;
    }

    public async Task<Transaction> CreateAsync(Transaction input)
    {
        var response = await SendCreateRequestAsync(input);

        await EnsureSuccessStatusCodeAsync(
            response,
            $"Failed to create transaction.");

        var transaction = await ReadTransactionAsync(response);

        LogTransactionCreated(transaction.Id);

        return transaction;
    }

    public async Task<bool> UpdateAsync(Guid id, Transaction input)
    {
        var response = await SendUpdateRequestAsync(id, input);

        await EnsureSuccessStatusCodeAsync(
            response,
            $"Failed to update transaction {id}.");

        LogTransactionUpdated(id);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await SendDeleteRequestAsync(id);

        await EnsureSuccessStatusCodeAsync(
            response,
            $"Failed to delete transaction {id}.");

        LogTransactionDeleted(id);

        return response.IsSuccessStatusCode;
    }

    private static string BuildTransactionsUrl(
        DateOnly? start,
        DateOnly? end)
    {
        var url = "/transactions";

        var query = CreateTransactionsQuery(start, end);

        return query.Count > 0
            ? QueryHelpers.AddQueryString(url, query)
            : url;
    }

    private static Dictionary<string, string?> CreateTransactionsQuery(
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
            query["start"] = start.Value.ToString("yyyy-MM-dd");
        }
    }

    private static void AddEndDateQuery(
        IDictionary<string, string?> query,
        DateOnly? end)
    {
        if (end.HasValue)
        {
            query["end"] = end.Value.ToString("yyyy-MM-dd");
        }
    }

    private async Task<Transaction[]> GetTransactionsFromApiAsync(string url)
    {
        return await httpClient.GetFromJsonAsync<Transaction[]>(url) ?? [];
    }

    private void LogTransactionsRetrieved(int transactionCount)
    {
        logger.LogInformation(
            "Retrieved {TransactionCount} transactions from transactions API",
            transactionCount);
    }

    private async Task<HttpResponseMessage> SendCreateRequestAsync(Transaction input)
    {
        return await httpClient.PostAsJsonAsync(
            "/transactions",
            input);
    }

    private async Task<HttpResponseMessage> SendUpdateRequestAsync(
        Guid id,
        Transaction input)
    {
        return await httpClient.PutAsJsonAsync(
            $"/transactions/{id}",
            input);
    }

    private async Task<HttpResponseMessage> SendDeleteRequestAsync(Guid id)
    {
        return await httpClient.DeleteAsync($"/transactions/{id}");
    }

    private async Task EnsureSuccessStatusCodeAsync(
        HttpResponseMessage response,
        string errorMessage)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await ApiErrorReader.ReadMessageAsync(response);

        LogApiFailure(
            response,
            errorMessage,
            message);

        throw new ApiClientException(message);
    }

    private void LogApiFailure(
        HttpResponseMessage response,
        string errorMessage,
        string message)
    {
        logger.LogWarning(
            "{ErrorMessage} Status code: {StatusCode}. Message: {Message}",
            errorMessage,
            response.StatusCode,
            message);
    }

    private async Task<Transaction> ReadTransactionAsync(
        HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<Transaction>()
            ?? throw new ApiClientException(
                "The transactions API returned an empty response.");
    }

    private void LogTransactionCreated(Guid transactionId)
    {
        logger.LogInformation(
            "Created transaction {TransactionId} through transactions API",
            transactionId);
    }

    private void LogTransactionUpdated(Guid transactionId)
    {
        logger.LogInformation(
            "Updated transaction {TransactionId} through transactions API",
            transactionId);
    }

    private void LogTransactionDeleted(Guid transactionId)
    {
        logger.LogInformation(
            "Deleted transaction {TransactionId} through transactions API",
            transactionId);
    }
}
