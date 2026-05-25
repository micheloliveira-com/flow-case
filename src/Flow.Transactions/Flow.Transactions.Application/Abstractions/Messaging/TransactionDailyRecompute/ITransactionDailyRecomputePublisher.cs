namespace Flow.Transactions.Application.Abstractions.Messaging.TransactionDailyRecompute;

public interface ITransactionDailyRecomputePublisher
{
    Task PublishAsync(
        TransactionDailyRecomputeMessage message);
}
