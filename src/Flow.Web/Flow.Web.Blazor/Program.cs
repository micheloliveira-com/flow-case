using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Flow.Web.Blazor;
using Flow.Web.Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor()
                .AddTransient<AuthorizationHandler>();

builder.Services.AddHttpClient<TransactionApiClient>(client =>
    {
        client.BaseAddress = new("https+http://transactionsapiservice");
    }).AddHttpMessageHandler<AuthorizationHandler>();

builder.Services.AddHttpClient<TransactionBalanceApiClient>(client =>
    {
        client.BaseAddress = new("https+http://reportsapiservice");
    }).AddHttpMessageHandler<AuthorizationHandler>();

var oidcScheme = OpenIdConnectDefaults.AuthenticationScheme;

builder.Services.AddAuthentication(oidcScheme)
    .AddKeycloakOpenIdConnect("keycloak", realm: "flow", oidcScheme, options =>
    {
        options.ClientId = "flow.web";
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.Scope.Add("flow:all");
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role; //"role";
        options.SaveTokens = true;

        //options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      
        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }

        // --- Role Configuration ---
        //options.Events = new OpenIdConnectEvents
        //{
        //    OnTicketReceived = context =>
        //    {
        //        var claimsPrincipal = context.Principal;
        //        if (claimsPrincipal != null)
        //        {
        //            var identity = claimsPrincipal.Identity as ClaimsIdentity;
        //            if (identity != null)
        //            {
        //                // Find all incoming claims where the type is "role" (from the token JSON key)
        //                var roleClaims = claimsPrincipal.FindAll("role").ToList();

        //                if (roleClaims.Any())
        //                {
        //                    // Remove the old "role" claims
        //                    roleClaims.ForEach(c => identity.RemoveClaim(c));

        //                    // Add new claims using the standard ClaimTypes.Role type
        //                    foreach (var roleClaim in roleClaims)
        //                    {
        //                        identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value, ClaimValueTypes.String, context.Options.Authority));
        //                    }
        //                }
        //            }
        //        }
        //        return Task.CompletedTask;
        //    }
        //};
    }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
    
builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Events.OnValidatePrincipal = context =>
        {
            var expiresAt =
                context.Properties?.GetTokenValue("expires_at");

            if (DateTimeOffset.TryParse(expiresAt, out var expires))
            {
                if (expires <= DateTime.UtcNow)
                {
                    context.RejectPrincipal();
                }
            }

            return Task.CompletedTask;
        };
    });

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();
app.MapLoginAndLogout();

app.Run();
