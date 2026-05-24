using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Flow.Reports.Workers;
using Flow.Reports.Infrastructure;

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
    ReportsDbContext db,
    DateOnly? start,
    DateOnly? end) =>
{
    var query = db.TransactionDailyBalance.AsNoTracking();
    query = query.Where(x => x.Balance != 0);

    if (start.HasValue)
        query = query.Where(x => x.Date >= start.Value);

    if (end.HasValue)
        query = query.Where(x => x.Date <= end.Value);

    query = query.OrderByDescending(x => x.Date);
    
    return await query.ToListAsync();
});

app.MapDefaultEndpoints();

app.Run();
