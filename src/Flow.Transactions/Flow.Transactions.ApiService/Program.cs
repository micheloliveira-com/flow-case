using Flow.Transactions.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Flow.Transactions.Workers;
using Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;

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
builder.Services.AddHostedService<TransactionDailyRecomputeWorker>();

builder.Services.AddScoped<ICreateTransactionService, CreateTransactionService>();
builder.Services.AddScoped<IGetTransactionsService, GetTransactionsService>();
builder.Services.AddScoped<IUpdateTransactionService, UpdateTransactionService>();

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
    ICreateTransactionService service) =>
{
    var tx = await service.ExecuteAsync(request);

    return Results.Created($"/transactions/{tx.Id}", tx);
});

app.MapGet("/transactions", async (
    [AsParameters] GetTransactionsRequest request,
    IGetTransactionsService service) =>
{
    return await service.ExecuteAsync(request);
});

app.MapPut("/transactions/{id:guid}", async (
    Guid id,
    UpdateTransactionRequest request,
    IUpdateTransactionService service) =>
{
    var tx = await service.ExecuteAsync(id, request);

    if (tx is null)
        return Results.NotFound();

    return Results.Ok(tx);
});

app.MapDelete("/transactions/{id:guid}", async (TransactionDbContext db, Guid id, IConnection connection) =>
{
    var tx = await db.Transactions.FirstOrDefaultAsync(x => x.Id == id);
    if (tx is null) return Results.NotFound();

    var date = tx.Date;

    db.Transactions.Remove(tx);

    await db.SaveChangesAsync();

    using var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(
        queue: "transaction-daily-recompute",
        durable: true,
        exclusive: false,
        autoDelete: false);

    await channel.BasicPublishAsync(
    exchange: "",
    routingKey: "transaction-daily-recompute",
    mandatory: false,
    basicProperties: new BasicProperties(),
    body: System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new
    {
        Date = date
    }));

    return Results.NoContent();
});



app.MapDefaultEndpoints();

app.Run();
