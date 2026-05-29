using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace Flow.Web.Blazor.Authentication;

public class AuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthorizationHandler> logger)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = GetHttpContext();

        var accessToken = await GetAccessTokenAsync(httpContext);

        if (HasAccessToken(accessToken))
        {
            SetAuthorizationHeader(
                request,
                accessToken!);
        }
        else
        {
            LogMissingAccessToken(request);
        }

        return await SendRequestAsync(
            request,
            cancellationToken);
    }

    private HttpContext GetHttpContext()
    {
        return httpContextAccessor.HttpContext ??
            throw new InvalidOperationException(
                """
                No HttpContext available from the IHttpContextAccessor.
                """);
    }

    private static async Task<string?> GetAccessTokenAsync(HttpContext httpContext)
    {
        return await httpContext.GetTokenAsync("access_token");
    }

    private static bool HasAccessToken(string? accessToken)
    {
        return !string.IsNullOrWhiteSpace(accessToken);
    }

    private static void SetAuthorizationHeader(
        HttpRequestMessage request,
        string accessToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken);
    }

    private void LogMissingAccessToken(HttpRequestMessage request)
    {
        logger.LogWarning(
            "No access token was available for outbound request to {RequestUri}",
            request.RequestUri);
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await base.SendAsync(
            request,
            cancellationToken);
    }
}
