using Flow.Transactions.Domain.Entities.Enums;

namespace Flow.Transactions.Application.UseCases.Transactions.CreateTransaction;

public sealed record CreateTransactionRequest(
    decimal Amount,
    TransactionType Type,
    DateOnly Date,
    string? Description
);