public sealed record CreateTransactionRequest(
    decimal Amount,
    TransactionType Type,
    DateOnly Date,
    string? Description
);