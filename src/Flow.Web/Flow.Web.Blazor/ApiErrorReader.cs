using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Web.Blazor;

internal static class ApiErrorReader
{
    public static async Task<string> ReadMessageAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return $"Request failed with status code {(int)response.StatusCode}.";

        try
        {
            var problem = JsonSerializer.Deserialize<ProblemDetails>(
                content,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            if (!string.IsNullOrWhiteSpace(problem?.Detail))
                return problem.Detail;

            if (!string.IsNullOrWhiteSpace(problem?.Title))
                return problem.Title;
        }
        catch (JsonException)
        {
            // Some infrastructure errors are plain text or HTML. Fall back to a short raw message.
        }

        return content.Length <= 300
            ? content
            : $"{content[..300]}...";
    }
}
