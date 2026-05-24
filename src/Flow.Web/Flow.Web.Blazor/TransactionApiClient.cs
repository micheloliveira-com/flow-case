
namespace Flow.Web.Blazor;

using Microsoft.AspNetCore.WebUtilities;

public class TransactionApiClient(HttpClient httpClient)
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

        return await httpClient.GetFromJsonAsync<Transaction[]>(
                   url)
               ?? [];
    }

    public async Task<Transaction?> CreateAsync(Transaction input)
    {
        var response = await httpClient.PostAsJsonAsync("/transactions", input);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<Transaction>();
    }

    public async Task<bool> UpdateAsync(Guid id, Transaction input)
    {
        var response = await httpClient.PutAsJsonAsync($"/transactions/{id}", input);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await httpClient.DeleteAsync($"/transactions/{id}");
        return response.IsSuccessStatusCode;
    }
}

public record Transaction(Guid Id, decimal Amount, TransactionType Type, DateOnly Date, string? Description);
public enum TransactionType
{
    Debit = 1,
    Credit = 2
}