
namespace Flow.Web.Blazor;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

public class TransactionApiClient(
    HttpClient httpClient,
    ILogger<TransactionApiClient> logger)
{
    public async Task<Transaction[]> GetTransactionsAsync(
        DateOnly? start,
        DateOnly? end)
    {
        var url = "/transactions";

        var query = new Dictionary<string, string?>();

        if (start.HasValue)
            query["start"] = start.Value.ToString("yyyy-MM-dd");

        if (end.HasValue)
            query["end"] = end.Value.ToString("yyyy-MM-dd");

        if (query.Count > 0)
            url = QueryHelpers.AddQueryString(url, query);

        var transactions = await httpClient.GetFromJsonAsync<Transaction[]>(
            url) ?? [];
        logger.LogInformation(
            "Retrieved {TransactionCount} transactions from transactions API",
            transactions.Length);

        return transactions;
    }

    public async Task<Transaction?> CreateAsync(Transaction input)
    {
        var response = await httpClient.PostAsJsonAsync("/transactions", input);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Failed to create transaction. Status code: {StatusCode}",
                response.StatusCode);
            return null;
        }

        var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
        logger.LogInformation(
            "Created transaction {TransactionId} through transactions API",
            transaction?.Id);

        return transaction;
    }

    public async Task<bool> UpdateAsync(Guid id, Transaction input)
    {
        var response = await httpClient.PutAsJsonAsync($"/transactions/{id}", input);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Failed to update transaction {TransactionId}. Status code: {StatusCode}",
                id,
                response.StatusCode);
        }
        else
        {
            logger.LogInformation(
                "Updated transaction {TransactionId} through transactions API",
                id);
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"/transactions/{id}");
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Failed to delete transaction {TransactionId}. Status code: {StatusCode}",
                id,
                response.StatusCode);
        }
        else
        {
            logger.LogInformation(
                "Deleted transaction {TransactionId} through transactions API",
                id);
        }

        return response.IsSuccessStatusCode;
    }
}

public record Transaction(Guid Id, decimal Amount, TransactionType Type, DateOnly Date, string? Description);
public enum TransactionType
{
    Debit = 1,
    Credit = 2
}
