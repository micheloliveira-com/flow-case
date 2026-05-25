

using Flow.Transactions.Domain.Entities.Enums;

namespace Flow.Transactions.Application.UseCases.Transactions.UpdateTransaction;
public sealed record UpdateTransactionRequest(
    decimal Amount,
    TransactionType Type,
    DateOnly Date,
    string? Description
);