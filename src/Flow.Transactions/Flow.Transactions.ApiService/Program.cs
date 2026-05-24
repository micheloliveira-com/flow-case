using Flow.Transactions.Infrastructure;
using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Flow.Transactions.Workers;
using Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;
using Flow.Transactions.Application.Abstractions.Persistence;

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
            options.Audience = "flow.transactions.api";

            // For development only - disable HTTPS metadata validation
            // In production, use explicit Authority configuration instead
            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

builder.AddNpgsqlDbContext<TransactionDbContext>("transactionsapiservicedb");

builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddScoped<ITransactionDailyRecomputePublisher, TransactionDailyRecomputePublisher>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<ICreateTransactionService, CreateTransactionService>();
builder.Services.AddScoped<IGetTransactionsService, GetTransactionsService>();
builder.Services.AddScoped<IUpdateTransactionService, UpdateTransactionService>();
builder.Services.AddScoped<IDeleteTransactionService, DeleteTransactionService>();


builder.Services.AddHostedService<TransactionDailyRecomputeWorker>();

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var context = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
await context.Database.MigrateAsync();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Transactions API is running.");

app.MapPost("/transactions", async (
    CreateTransactionRequest request,
    ICreateTransactionService service,
    CancellationToken cancellationToken) =>
{
    var tx = await service.ExecuteAsync(request, cancellationToken);

    return Results.Created($"/transactions/{tx.Id}", tx);
});

app.MapGet("/transactions", async (
    [AsParameters] GetTransactionsRequest request,
    IGetTransactionsService service,
    CancellationToken cancellationToken) =>
{
    return await service.ExecuteAsync(request, cancellationToken);
});

app.MapPut("/transactions/{id:guid}", async (
    Guid id,
    UpdateTransactionRequest request,
    IUpdateTransactionService service,
    CancellationToken cancellationToken) =>
{
    var tx = await service.ExecuteAsync(id, request, cancellationToken);

    if (tx is null)
        return Results.NotFound();

    return Results.Ok(tx);
});

app.MapDelete("/transactions/{id:guid}", async (
    [AsParameters] DeleteTransactionRequest request,
    IDeleteTransactionService service,
    CancellationToken cancellationToken) =>
{
    var deleted = await service.ExecuteAsync(request, cancellationToken);

    if (!deleted)
        return Results.NotFound();

    return Results.NoContent();
});



app.MapDefaultEndpoints();

app.Run();
