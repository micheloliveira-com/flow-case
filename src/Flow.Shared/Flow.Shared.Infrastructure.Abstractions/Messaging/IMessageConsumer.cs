using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Shared.Infrastructure.Abstractions.Messaging;

public delegate Task MessageHandler<in T>(T message);

/// <summary>
/// Abstraction for message consumption (lightweight, transport-agnostic).
/// Only responsible for subscribing and dispatching messages.
/// </summary>
public interface IMessageConsumer
{
    Task SubscribeAsync<T>(
        string queue,
        MessageHandler<T> handler,
        CancellationToken cancellationToken);
}