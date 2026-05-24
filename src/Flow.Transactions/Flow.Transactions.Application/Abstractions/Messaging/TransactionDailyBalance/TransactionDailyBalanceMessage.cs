

namespace Flow.Transactions.Infrastructure.Messaging.Messages.TransactionDailyBalance;

public sealed record TransactionDailyBalanceMessage(
    DateOnly Date,
    decimal Balance,
    DateTime ProcessedAt);