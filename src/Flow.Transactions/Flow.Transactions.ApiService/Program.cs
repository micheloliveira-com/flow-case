using Flow.Transactions.Infrastructure;
using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;
using Flow.Transactions.Infrastructure.Messaging.RabbitMq;
using Flow.Transactions.Infrastructure.Messaging.Consumers;
using Flow.Transactions.Application.UseCases.Transactions.CreateTransaction;
using Flow.Transactions.Application.UseCases.Transactions.GetTransactions;
using Flow.Transactions.Application.UseCases.Transactions.DeleteTransaction;
using Flow.Transactions.Infrastructure.Persistence;
using Flow.Transactions.ApiService.Workers;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyBalance;
using Flow.Transactions.Infrastructure.Messaging.Publishers;
using Flow.Transactions.Infrastructure.Messaging;
using Flow.Shared.Infrastructure.Abstractions.Messaging;
using Flow.Transactions.Infrastructure.Persistence.Repositories;
using Flow.Transactions.ApiService.Endpoints;
using Flow.Transactions.ApiService.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

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

builder.AddSeqEndpoint(connectionName: "seq");

builder.AddNpgsqlDbContext<TransactionDbContext>("transactionsapiservicedb");

builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddScoped<IMessageConsumer, RabbitMqConsumer>();
builder.Services.AddScoped<ITransactionDailyRecomputeConsumer, TransactionDailyRecomputeConsumer>();
builder.Services.AddScoped<ITransactionDailyBalancePublisher, TransactionDailyBalancePublisher>();
builder.Services.AddScoped<ITransactionDailyRecomputePublisher, TransactionDailyRecomputePublisher>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IExecuteTransactionDailyRecomputeService, ExecuteTransactionDailyRecomputeService>();

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

app.MapTransactionEndpoints();

app.MapDefaultEndpoints();

app.Run();
