using System.Text;
using System.Text.Json;
using Flow.Transactions.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Flow.Shared.Tests.Messaging;

public sealed class RabbitMqConsumerTests
{
    private const string QueueName = "test-queue";
    private const ulong DeliveryTag = 42;
    private sealed record TestMessage(string Name);
    
    [Fact]
    public async Task SubscribeAsync_ShouldDeclareQueueAndStartConsumer()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        var context = CreateContext(cancellationTokenSource.Token);
        var sut = CreateSut(context.Connection);

        // Act
        await sut.SubscribeAsync<TestMessage>(
            QueueName,
            _ => Task.CompletedTask,
            cancellationTokenSource.Token);

        // Assert
        VerifyContext(context);
    }

    [Fact]
    public async Task SubscribeAsync_WhenMessageIsValid_ShouldInvokeHandlerAndAck()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        var handled = false;
        var context = CreateContext(cancellationTokenSource.Token);

        context.Channel
            .Setup(x => x.BasicAckAsync(DeliveryTag, false, cancellationTokenSource.Token))
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        var sut = CreateSut(context.Connection);

        await sut.SubscribeAsync<TestMessage>(
            QueueName,
            message =>
            {
                handled = message.Name == "hello";
                return Task.CompletedTask;
            },
            cancellationTokenSource.Token);

        // Act
        await DeliverMessageAsync(
            context.Consumer,
            new TestMessage("hello"),
            cancellationTokenSource.Token);

        // Assert
        Assert.True(handled);
        VerifyContext(context);
    }

    [Fact]
    public async Task SubscribeAsync_WhenMessageIsInvalid_ShouldNackWithoutRequeue()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        var handled = false;
        var context = CreateContext(cancellationTokenSource.Token);

        context.Channel
            .Setup(x => x.BasicNackAsync(DeliveryTag, false, false, cancellationTokenSource.Token))
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        var sut = CreateSut(context.Connection);

        await sut.SubscribeAsync<TestMessage>(
            QueueName,
            _ =>
            {
                handled = true;
                return Task.CompletedTask;
            },
            cancellationTokenSource.Token);

        // Act
        await DeliverRawMessageAsync(
            context.Consumer,
            "null",
            cancellationTokenSource.Token);

        // Assert
        Assert.False(handled);
        VerifyContext(context);
    }

    private static RabbitMqConsumer CreateSut(Mock<IConnection> connection)
    {
        return new RabbitMqConsumer(connection.Object, NullLogger<RabbitMqConsumer>.Instance);
    }

    private static RabbitMqConsumerTestContext CreateContext(CancellationToken cancellationToken)
    {
        var connection = new Mock<IConnection>(MockBehavior.Strict);
        var channel = new Mock<IChannel>(MockBehavior.Strict);

        IAsyncBasicConsumer? consumer = null;

        connection
            .Setup(x => x.CreateChannelAsync(null, cancellationToken))
            .ReturnsAsync(channel.Object)
            .Verifiable();

        channel
            .Setup(x => x.QueueDeclareAsync(
                QueueName,
                true,
                false,
                false,
                It.IsAny<IDictionary<string, object?>>(),
                false,
                false,
                cancellationToken))
            .Returns(Task.FromResult(new QueueDeclareOk(QueueName, 0, 0)))
            .Verifiable();

        channel
            .Setup(x => x.BasicConsumeAsync(
                QueueName,
                false,
                It.IsAny<string>(),
                false,
                false,
                It.IsAny<IDictionary<string, object?>>(),
                It.IsAny<IAsyncBasicConsumer>(),
                cancellationToken))
            .Callback<string, bool, string, bool, bool, IDictionary<string, object?>, IAsyncBasicConsumer, CancellationToken>(
                (_, _, _, _, _, _, capturedConsumer, _) => consumer = capturedConsumer)
            .ReturnsAsync("consumer-tag")
            .Verifiable();

        return new RabbitMqConsumerTestContext(connection, channel, () => consumer);
    }

    private static async Task DeliverMessageAsync(
        IAsyncBasicConsumer consumer,
        TestMessage message,
        CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        await DeliverRawMessageAsync(
            consumer,
            body,
            cancellationToken);
    }

    private static async Task DeliverRawMessageAsync(
        IAsyncBasicConsumer consumer,
        string payload,
        CancellationToken cancellationToken)
    {
        var body = Encoding.UTF8.GetBytes(payload);

        await DeliverRawMessageAsync(
            consumer,
            body,
            cancellationToken);
    }

    private static async Task DeliverRawMessageAsync(
        IAsyncBasicConsumer consumer,
        byte[] body,
        CancellationToken cancellationToken)
    {
        await ((AsyncEventingBasicConsumer)consumer).HandleBasicDeliverAsync(
            "consumer-tag",
            DeliveryTag,
            false,
            string.Empty,
            QueueName,
            Mock.Of<IReadOnlyBasicProperties>(),
            body,
            cancellationToken);
    }

    private static void VerifyContext(RabbitMqConsumerTestContext context)
    {
        Assert.NotNull(context.Consumer);

        context.Connection.Verify();
        context.Channel.Verify();
        context.Connection.VerifyNoOtherCalls();
        context.Channel.VerifyNoOtherCalls();
    }
}
