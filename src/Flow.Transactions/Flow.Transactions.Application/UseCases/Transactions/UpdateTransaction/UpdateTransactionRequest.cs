

public sealed record UpdateTransactionRequest(
    decimal Amount,
    TransactionType Type,
    DateOnly Date,
    string? Description
);