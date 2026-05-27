using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flow.Shared.Infrastructure.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Flow.Transactions.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqConsumer(
    IConnection connection,
    ILogger<RabbitMqConsumer> logger) : IMessageConsumer
{
    public async Task SubscribeAsync<T>(
        string queue,
        MessageHandler<T> handler,
        CancellationToken cancellationToken)
    {
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        logger.LogInformation(
            "Starting RabbitMQ consumer for queue {Queue}",
            queue);

        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.Span);
                var message = JsonSerializer.Deserialize<T>(json);

                if (message is null)
                {
                    logger.LogWarning(
                        "Received invalid RabbitMQ message on queue {Queue}. Delivery tag: {DeliveryTag}",
                        queue,
                        args.DeliveryTag);
                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: cancellationToken);
                    return;
                }

                await handler(message);

                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken);
                logger.LogInformation(
                    "Processed RabbitMQ message from queue {Queue}. Delivery tag: {DeliveryTag}",
                    queue,
                    args.DeliveryTag);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to process RabbitMQ message from queue {Queue}. Delivery tag: {DeliveryTag}",
                    queue,
                    args.DeliveryTag);
                await channel.BasicNackAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }
}
