
using Moq;
using RabbitMQ.Client;

namespace Flow.Shared.Tests.Messaging;
public sealed class RabbitMqConsumerTestContext
{
    private readonly Func<IAsyncBasicConsumer?> _consumerAccessor;

    public RabbitMqConsumerTestContext(
        Mock<IConnection> connection,
        Mock<IChannel> channel,
        Func<IAsyncBasicConsumer?> consumerAccessor)
    {
        Connection = connection;
        Channel = channel;
        _consumerAccessor = consumerAccessor;
    }

    public Mock<IConnection> Connection { get; }

    public Mock<IChannel> Channel { get; }

    public IAsyncBasicConsumer Consumer =>
        _consumerAccessor() ?? throw new InvalidOperationException("Consumer was not initialized.");
}
