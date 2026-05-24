using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Flow.Reports.Domain.Entities;
using Flow.Reports.Infrastructure;

namespace Flow.Reports.Workers;

public sealed class TransactionDailyBalanceWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TransactionDailyBalanceWorker> logger,
    IConnection rabbitConnection)
    : BackgroundService
{
    private const string DailyBalanceQueue = "transaction-daily-balance";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await rabbitConnection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: DailyBalanceQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.Span);

                var message = JsonSerializer.Deserialize<TransactionDailyBalanceMessage>(json);

                if (message is null)
                {
                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                using var scope = scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();

                var dailyBalance = new TransactionDailyBalanceMessage
                {
                    Date = message.Date,
                    Balance = message.Balance,
                    ProcessedAt = message.ProcessedAt
                };

                var currentDailyBalance = await db.TransactionDailyBalance
                    .FirstOrDefaultAsync(
                        x => x.Date == dailyBalance.Date,
                        stoppingToken);

                if (currentDailyBalance is not null
                    && currentDailyBalance.ProcessedAt >= dailyBalance.ProcessedAt)
                {
                    logger.LogInformation(
                        "Ignoring outdated transaction daily balance for {Date}. Current ProcessedAt: {CurrentProcessedAt}. Incoming ProcessedAt: {IncomingProcessedAt}",
                        dailyBalance.Date,
                        currentDailyBalance.ProcessedAt,
                        dailyBalance.ProcessedAt);

                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                if (currentDailyBalance is null)
                {
                    currentDailyBalance = new TransactionDailyBalance
                    {
                        Id = Guid.NewGuid(),
                        Date = dailyBalance.Date
                    };

                    await db.TransactionDailyBalance.AddAsync(
                        currentDailyBalance,
                        stoppingToken);
                }

                currentDailyBalance.Balance = dailyBalance.Balance;
                currentDailyBalance.ProcessedAt = dailyBalance.ProcessedAt;

                await db.SaveChangesAsync(stoppingToken);

                logger.LogInformation(
                    "Persisted transaction daily balance for {Date} with balance {Balance} and ProcessedAt {ProcessedAt}",
                    currentDailyBalance.Date,
                    currentDailyBalance.Balance,
                    currentDailyBalance.ProcessedAt);

                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing transaction daily balance");

                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: DailyBalanceQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

public sealed class TransactionDailyBalanceMessage
{
    public DateOnly Date { get; set; }

    public decimal Balance { get; set; }

    public DateTime ProcessedAt { get; set; }
}