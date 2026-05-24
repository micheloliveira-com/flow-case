namespace Flow.Shared.Application.Abstractions.Messaging;

public sealed record TransactionDailyBalanceMessage(
    DateOnly Date,
    decimal Balance,
    DateTime ProcessedAt);