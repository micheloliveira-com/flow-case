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
        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: routingKey,
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body);
        logger.LogInformation(
            "Published RabbitMQ message to routing key {RoutingKey} with payload type {MessageType}",
            routingKey,
            typeof(T).Name);
    }
}
