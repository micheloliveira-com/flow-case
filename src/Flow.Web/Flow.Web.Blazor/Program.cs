using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Flow.Web.Blazor.Authentication;
using Flow.Web.Blazor.Clients;
using Flow.Web.Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

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
        options.UseTokenLifetime = true;

        //options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      
        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }
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

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();
app.MapLoginAndLogout();

app.Run();
