using Flow.Transactions.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Flow.Transactions.Workers;

public sealed class TransactionDailyRecomputeWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TransactionDailyRecomputeWorker> logger,
    IConnection rabbitConnection)
    : BackgroundService
{
    private const string DailyRecomputeQueue = "transaction-daily-recompute";
    private const string DailyBalanceQueue = "transaction-daily-balance";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await rabbitConnection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: DailyRecomputeQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
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

                var message = JsonSerializer.Deserialize<TransactionDailyRecomputeMessage>(json);

                if (message is null)
                {
                    await channel.BasicAckAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                using var scope = scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

                var date = DateOnly.FromDateTime(
                    message.Date.ToUniversalTime());

                var balance = await db.Transactions
                    .AsNoTracking()
                    .Where(x => x.Date == date)
                    .SumAsync(x =>
                            x.Type == TransactionType.Credit
                                ? x.Amount
                                : -x.Amount,
                        cancellationToken: stoppingToken);

                var dailyBalance = new TransactionDailyBalanceMessage
                {
                    Date = date,
                    Balance = balance,
                    ProcessedAt = DateTime.UtcNow
                };

                var dailyBalanceBody = Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(dailyBalance));

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: DailyBalanceQueue,
                    mandatory: false,
                    body: dailyBalanceBody,
                    cancellationToken: stoppingToken);

                logger.LogInformation(
                    "Published transaction daily balance for {Date} with balance {Balance}",
                    date,
                    balance);

                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing transaction daily recompute");

                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: DailyRecomputeQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

public sealed class TransactionDailyRecomputeMessage
{
    public DateTime Date { get; set; }
}

public sealed class TransactionDailyBalanceMessage
{
    public DateOnly Date { get; set; }

    public decimal Balance { get; set; }

    public DateTime ProcessedAt { get; set; }
}