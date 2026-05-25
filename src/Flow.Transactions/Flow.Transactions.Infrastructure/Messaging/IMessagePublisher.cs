namespace Flow.Transactions.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(
        string routingKey,
        T message);
}