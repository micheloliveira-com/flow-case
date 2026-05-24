
namespace Flow.Web.Blazor;

using Microsoft.AspNetCore.WebUtilities;

public class TransactionApiClient(HttpClient httpClient)
{
    public async Task<Transaction[]> GetTransactionsAsync(
        DateOnly? start,
        DateOnly? end,
        CancellationToken cancellationToken = default)
    {
        var url = "/transactions";

        var query = new Dictionary<string, string?>();

        if (start.HasValue)
            query["start"] = start.Value.ToString("yyyy-MM-dd");

        if (end.HasValue)
            query["end"] = end.Value.ToString("yyyy-MM-dd");

        if (query.Count > 0)
            url = QueryHelpers.AddQueryString(url, query);

        return await httpClient.GetFromJsonAsync<Transaction[]>(
                   url,
                   cancellationToken)
               ?? [];
    }

    public async Task<Transaction?> CreateAsync(Transaction input, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/transactions", input, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<Transaction>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateAsync(Guid id, Transaction input, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/transactions/{id}", input, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/transactions/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

public record Transaction(Guid Id, decimal Amount, TransactionType Type, DateOnly Date, string? Description);
public enum TransactionType
{
    Debit = 1,
    Credit = 2
}