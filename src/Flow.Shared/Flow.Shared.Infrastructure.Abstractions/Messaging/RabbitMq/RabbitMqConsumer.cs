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
    ILogger<RabbitMqConsumer> logger)
    : IMessageConsumer
{
    public async Task SubscribeAsync<T>(
        string queue,
        MessageHandler<T> handler,
        CancellationToken cancellationToken)
    {
        var channel = await CreateChannelAsync(cancellationToken);

        LogConsumerStarted(queue);

        await DeclareQueueAsync(
            channel,
            queue,
            cancellationToken);

        var consumer = CreateConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            await ProcessMessageAsync(
                channel,
                queue,
                handler,
                args,
                cancellationToken);
        };

        await StartConsumerAsync(
            channel,
            queue,
            consumer,
            cancellationToken);
    }

    private async Task<IChannel> CreateChannelAsync(
        CancellationToken cancellationToken)
    {
        return await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);
    }

    private void LogConsumerStarted(string queue)
    {
        logger.LogInformation(
            "Starting RabbitMQ consumer for queue {Queue}",
            queue);
    }

    private static async Task DeclareQueueAsync(
        IChannel channel,
        string queue,
        CancellationToken cancellationToken)
    {
        await channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    private static AsyncEventingBasicConsumer CreateConsumer(IChannel channel)
    {
        return new AsyncEventingBasicConsumer(channel);
    }

    private async Task ProcessMessageAsync<T>(
        IChannel channel,
        string queue,
        MessageHandler<T> handler,
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = DeserializeMessage<T>(args.Body.Span);

            if (message is null)
            {
                await HandleInvalidMessageAsync(
                    channel,
                    queue,
                    args,
                    cancellationToken);

                return;
            }

            await handler(message);

            await AcknowledgeMessageAsync(
                channel,
                queue,
                args,
                cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleProcessingErrorAsync(
                channel,
                queue,
                args,
                ex,
                cancellationToken);
        }
    }

    private static T? DeserializeMessage<T>(ReadOnlySpan<byte> body)
    {
        var json = Encoding.UTF8.GetString(body);

        return JsonSerializer.Deserialize<T>(json);
    }

    private async Task HandleInvalidMessageAsync(
        IChannel channel,
        string queue,
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
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
    }

    private async Task AcknowledgeMessageAsync(
        IChannel channel,
        string queue,
        BasicDeliverEventArgs args,
        CancellationToken cancellationToken)
    {
        await channel.BasicAckAsync(
            deliveryTag: args.DeliveryTag,
            multiple: false,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Processed RabbitMQ message from queue {Queue}. Delivery tag: {DeliveryTag}",
            queue,
            args.DeliveryTag);
    }

    private async Task HandleProcessingErrorAsync(
        IChannel channel,
        string queue,
        BasicDeliverEventArgs args,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Failed to process RabbitMQ message from queue {Queue}. Delivery tag: {DeliveryTag}",
            queue,
            args.DeliveryTag);

        await channel.BasicNackAsync(
            deliveryTag: args.DeliveryTag,
            multiple: false,
            requeue: true,
            cancellationToken: cancellationToken);
    }

    private static async Task StartConsumerAsync(
        IChannel channel,
        string queue,
        AsyncEventingBasicConsumer consumer,
        CancellationToken cancellationToken)
    {
        await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }
}
