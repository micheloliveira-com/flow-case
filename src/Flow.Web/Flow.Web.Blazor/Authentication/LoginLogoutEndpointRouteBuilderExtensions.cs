using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Flow.Web.Blazor.Authentication;

internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    internal static IEndpointConventionBuilder MapLoginAndLogout(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("authentication");

        group.MapGet(pattern: "/login", OnLogin).AllowAnonymous();
        group.MapPost(pattern: "/logout", OnLogout);

        return group;
    }

    private static ChallengeHttpResult OnLogin() =>
        TypedResults.Challenge(properties: new AuthenticationProperties
        {
            RedirectUri = "/"
        });

    private static SignOutHttpResult OnLogout() =>
        TypedResults.SignOut(properties: new AuthenticationProperties
        {
            RedirectUri = "/"
        },
        [
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        ]);
}
