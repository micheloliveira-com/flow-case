using Flow.Transactions.Application.Abstractions.Messaging;
using Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;
using Flow.Transactions.Application.Abstractions.Persistence;
using Flow.Transactions.Application.UseCases.DailyRecompute.ExecuteTransactionDailyRecompute;
using Flow.Transactions.Infrastructure.Messaging.Messages.TransactionDailyBalance;

public sealed class ExecuteTransactionDailyRecomputeService : IExecuteTransactionDailyRecomputeService
{
    private readonly ITransactionRepository _repository;
    private readonly IMessageConsumer _consumer;
    private readonly ITransactionDailyBalancePublisher _publisher;

    public ExecuteTransactionDailyRecomputeService(
        ITransactionRepository repository,
        IMessageConsumer consumer,
        ITransactionDailyBalancePublisher publisher)
    {
        _repository = repository;
        _consumer = consumer;
        _publisher = publisher;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        const string queue = "transaction-daily-recompute";

        await _consumer.SubscribeAsync<TransactionDailyRecomputeMessage>(
            queue,
            async message =>
            {
                if (message is null)
                    return;

                var date = message.Date;

                var transactions = await _repository.GetAsync(date, date, cancellationToken);

                var balance = transactions.Sum(x =>
                    x.Type == TransactionType.Credit
                        ? x.Amount
                        : -x.Amount);

                await _publisher.PublishAsync(
                    new TransactionDailyBalanceMessage(date, balance, DateTime.UtcNow),
                    cancellationToken);
            },
            cancellationToken);
    }
}