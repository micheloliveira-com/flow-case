using System.Text.Json;
using Flow.Transactions.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RabbitMQ.Client;

namespace Flow.Transactions.Tests.Infrastructure;

public sealed class RabbitMqPublisherTests
{
    [Fact]
    public async Task PublishAsync_ShouldCreateChannelAndPublishSerializedMessage()
    {
        // Arrange
        var connection = new Mock<IConnection>(MockBehavior.Strict);
        var channel = new Mock<IChannel>(MockBehavior.Strict);
        var message = new TestMessage("transaction-created", 150m);
        var expectedBody = JsonSerializer.SerializeToUtf8Bytes(message);

        connection
            .Setup(x => x.CreateChannelAsync(
                It.IsAny<CreateChannelOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(channel.Object)
            .Verifiable();

        channel
            .Setup(x => x.BasicPublishAsync(
                "",
                "transaction-created",
                false,
                It.IsAny<BasicProperties>(),
                It.Is<ReadOnlyMemory<byte>>(body => body.ToArray().SequenceEqual(expectedBody)),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        channel
            .Setup(x => x.Dispose())
            .Verifiable();

        var publisher = new RabbitMqPublisher(
            connection.Object,
            NullLogger<RabbitMqPublisher>.Instance);

        // Act
        await publisher.PublishAsync("transaction-created", message);

        // Assert
        connection.Verify();
        channel.Verify();
        connection.VerifyNoOtherCalls();
        channel.VerifyNoOtherCalls();
    }

    private sealed record TestMessage(string Name, decimal Amount);
}
