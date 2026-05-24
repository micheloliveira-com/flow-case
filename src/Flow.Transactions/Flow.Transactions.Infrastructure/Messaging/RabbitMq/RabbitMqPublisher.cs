using System.Text.Json;
using Flow.Transactions.Application.Abstractions.Messaging;
using RabbitMQ.Client;

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