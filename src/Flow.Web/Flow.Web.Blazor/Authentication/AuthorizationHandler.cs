using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace Flow.Web.Blazor.Authentication;

public class AuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthorizationHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext ??
            throw new InvalidOperationException("""
                No HttpContext available from the IHttpContextAccessor.
                """);

        var accessToken = await httpContext.GetTokenAsync("access_token");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        else
        {
            logger.LogWarning(
                "No access token was available for outbound request to {RequestUri}",
                request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
