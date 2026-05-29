using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Web.Blazor.Clients;

internal static class ApiErrorReader
{
    public static async Task<string> ReadMessageAsync(HttpResponseMessage response)
    {
        var content = await ReadContentAsync(response);

        if (IsEmptyContent(content))
        {
            return CreateEmptyContentMessage(response);
        }

        var problemMessage = TryReadProblemDetailsMessage(content);

        if (HasMessage(problemMessage))
        {
            return problemMessage!;
        }

        return CreateFallbackMessage(content);
    }

    private static async Task<string> ReadContentAsync(
        HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    private static bool IsEmptyContent(string content)
    {
        return string.IsNullOrWhiteSpace(content);
    }

    private static string CreateEmptyContentMessage(
        HttpResponseMessage response)
    {
        return $"Request failed with status code {(int)response.StatusCode}.";
    }

    private static string? TryReadProblemDetailsMessage(string content)
    {
        try
        {
            var problem = DeserializeProblemDetails(content);

            return GetProblemMessage(problem);
        }
        catch (Exception exception)
        {
            return $"Failed to deserialize API problem details response: {exception}";
        }
    }

    private static ProblemDetails? DeserializeProblemDetails(string content)
    {
        return JsonSerializer.Deserialize<ProblemDetails>(
            content,
            CreateSerializerOptions());
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        return new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    private static string? GetProblemMessage(ProblemDetails? problem)
    {
        if (HasMessage(problem?.Detail))
        {
            return problem!.Detail;
        }

        if (HasMessage(problem?.Title))
        {
            return problem!.Title;
        }

        return null;
    }

    private static bool HasMessage(string? message)
    {
        return !string.IsNullOrWhiteSpace(message);
    }

    private static string CreateFallbackMessage(string content)
    {
        return content.Length <= 300
            ? content
            : $"{content[..300]}...";
    }
}
