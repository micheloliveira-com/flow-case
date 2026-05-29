using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Flow.Transactions.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqPublisher(
    IConnection connection,
    ILogger<RabbitMqPublisher> logger)
    : IMessagePublisher
{
    public async Task PublishAsync<T>(
        string routingKey,
        T message)
    {
        using var channel = await connection.CreateChannelAsync();

        var body = SerializeMessage(message);

        await PublishMessageAsync(
            channel,
            routingKey,
            body);

        LogPublishedMessage<T>(routingKey);
    }

    private static byte[] SerializeMessage<T>(T message)
    {
        return JsonSerializer.SerializeToUtf8Bytes(message);
    }

    private static async Task PublishMessageAsync(
        IChannel channel,
        string routingKey,
        byte[] body)
    {
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body);
    }

    private void LogPublishedMessage<T>(string routingKey)
    {
        logger.LogInformation(
            "Published RabbitMQ message to routing key {RoutingKey} with payload type {MessageType}",
            routingKey,
            typeof(T).Name);
    }
}
