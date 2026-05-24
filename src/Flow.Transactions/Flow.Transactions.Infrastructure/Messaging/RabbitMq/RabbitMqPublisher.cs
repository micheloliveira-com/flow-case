using System.Text.Json;
using RabbitMQ.Client;

namespace Flow.Transactions.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqPublisher(
    IConnection connection)
    : IMessagePublisher
{
    public async Task PublishAsync<T>(
        string routingKey,
        T message)
    {
        using var channel = await connection.CreateChannelAsync();

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: routingKey,
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: JsonSerializer.SerializeToUtf8Bytes(message));
    }
}