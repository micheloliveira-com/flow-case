

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Transactions.Application.Abstractions.Messaging;

/// <summary>
/// Abstraction for message consumption (lightweight, transport-agnostic).
/// Only responsible for subscribing and dispatching messages.
/// </summary>
public interface IMessageConsumer
{
    Task SubscribeAsync<T>(
        string queue,
        Func<T, Task> handler,
        CancellationToken cancellationToken = default);
}