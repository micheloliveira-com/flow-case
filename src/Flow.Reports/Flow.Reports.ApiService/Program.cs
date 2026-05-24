using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Flow.Reports.Workers;
using Flow.Reports.Infrastructure;
using Flow.Reports.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
     serviceName: "keycloak",
       realm: "flow",
        configureOptions: options =>
        {
            options.Audience = "flow.reports.api";

            // For development only - disable HTTPS metadata validation
            // In production, use explicit Authority configuration instead
            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

builder.AddNpgsqlDbContext<ReportsDbContext>("reportsapiservicedb");

builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.AddRabbitMQClient("rabbitmq");
builder.Services.AddHostedService<TransactionDailyBalanceWorker>();

builder.Services.AddScoped<IGetTransactionDailyBalance, GetTransactionDailyBalance>();
builder.Services.AddScoped<ITransactionDailyBalanceRepository, TransactionDailyBalanceRepository>();

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var context = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
await context.Database.MigrateAsync();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Reports API is running.");

app.MapGet("/transaction_daily_balance", async (
    IGetTransactionDailyBalance service,
    [AsParameters] GetTransactionDailyBalanceRequest request,
    CancellationToken cancellationToken) =>
{
    return await service.ExecuteAsync(request, cancellationToken);
});

app.MapDefaultEndpoints();

app.Run();
